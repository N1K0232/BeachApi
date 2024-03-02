using System.Text;
using BeachApi.Authentication;
using BeachApi.Authentication.Entities;
using BeachApi.Authentication.Handlers;
using BeachApi.Authentication.Models;
using BeachApi.Authentication.Requirements;
using BeachApi.BusinessLayer.MapperProfiles;
using BeachApi.BusinessLayer.Services;
using BeachApi.BusinessLayer.Settings;
using BeachApi.BusinessLayer.StartupServices;
using BeachApi.Contracts;
using BeachApi.DataAccessLayer;
using BeachApi.Extensions;
using BeachApi.MultiTenant;
using BeachApi.Services;
using BeachApi.StorageProviders.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using OperationResults.AspNetCore;
using Serilog;
using TinyHelpers.AspNetCore.Extensions;
using TinyHelpers.AspNetCore.Swagger;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services, builder.Configuration, builder.Host, builder.Environment);

var app = builder.Build();
Configure(app, app.Environment, app.Services);

await app.RunAsync();

void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostBuilder host, IWebHostEnvironment environment)
{
    var appSettings = services.ConfigureAndGet<AppSettings>(configuration, nameof(AppSettings));
    var jwtSettings = services.ConfigureAndGet<JwtSettings>(configuration, nameof(JwtSettings));
    var sendinblueSettings = services.ConfigureAndGet<SendinblueSettings>(configuration, nameof(SendinblueSettings));

    services.Configure<AdministratorUser>(configuration.GetSection("AdministratorUserOptions"));
    services.Configure<PowerUser>(configuration.GetSection("PowerUserOptions"));

    host.UseSerilog((hostingContext, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
    });

    services.AddOperationResult(options =>
    {
        options.ErrorResponseFormat = ErrorResponseFormat.List;
    });

    services.AddHttpContextAccessor();
    services.AddMemoryCache();
    services.AddRequestLocalization(appSettings.SupportedCultures);

    services.AddControllers();
    services.AddEndpointsApiExplorer();

    services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Insert JWT token with the \"Bearer \" prefix",
            Name = HeaderNames.Authorization,
            Type = SecuritySchemeType.ApiKey
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme
                    }
                },
                Array.Empty<string>()
            }
        });

        options.AddAcceptLanguageHeader();
        options.AddDefaultResponse();
    });

    services.AddDataProtection().PersistKeysToDbContext<AuthenticationDbContext>();

    services.AddAutoMapper(typeof(UserMapperProfile).Assembly);

    services.AddFluentEmail(sendinblueSettings.FromEmailAddress).AddSendinblueSender();

    services.AddSqlServer<AuthenticationDbContext>(configuration.GetConnectionString("AuthConnection"));
    services.AddDbContext<IDataContext, DataContext>((services, options) =>
    {
        var tenantService = services.GetRequiredService<ITenantService>();
        var tenant = tenantService.Get();

        options.UseSqlServer(tenant.ConnectionString);
    });

    services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
    })
    .AddEntityFrameworkStores<AuthenticationDbContext>()
    .AddDefaultTokenProviders();

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecurityKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    services.AddAuthorization(options =>
    {
        var policyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
        policyBuilder.Requirements.Add(new UserActiveRequirement());

        options.FallbackPolicy = options.DefaultPolicy = policyBuilder.Build();
    });

    services.AddScoped<IAuthorizationHandler, UserActiveHandler>();
    services.AddScoped<IUserService, HttpUserService>();
    services.AddScoped<ITenantService, TenantService>();

    if (environment.IsDevelopment())
    {
        services.AddFileSystemStorage(options =>
        {
            options.SiteRootFolder = environment.ContentRootPath;
            options.StorageFolder = "D:\\BeachApi\\attachments";
        });
    }
    else
    {
        services.AddAzureStorage((services, options) =>
        {
            var tenantService = services.GetRequiredService<ITenantService>();
            var tenant = tenantService.Get();

            options.ConnectionString = tenant.StorageConnectionString;
            options.ContainerName = tenant.ContainerName;
        });
    }

    services.Scan(scan => scan.FromAssemblyOf<IdentityService>()
        .AddClasses(classes => classes.InNamespaceOf<IdentityService>())
        .AsImplementedInterfaces()
        .WithScopedLifetime());

    services.AddHostedService<IdentityRoleStartupService>();
    services.AddHostedService<IdentityUserStartupService>();
}

void Configure(IApplicationBuilder app, IWebHostEnvironment environment, IServiceProvider services)
{
    app.UseHttpsRedirection();
    app.UseRequestLocalization();

    if (environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}