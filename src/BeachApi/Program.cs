using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning.ApiExplorer;
using BeachApi.Authentication;
using BeachApi.Authentication.Entities;
using BeachApi.Authentication.Handlers;
using BeachApi.Authentication.Models;
using BeachApi.Authentication.Requirements;
using BeachApi.BusinessLayer.BackgroundServices;
using BeachApi.BusinessLayer.MapperProfiles;
using BeachApi.BusinessLayer.Services;
using BeachApi.BusinessLayer.Settings;
using BeachApi.BusinessLayer.StartupServices;
using BeachApi.BusinessLayer.Validations;
using BeachApi.Contracts;
using BeachApi.DataAccessLayer;
using BeachApi.Extensions;
using BeachApi.MultiTenant;
using BeachApi.Services;
using BeachApi.StorageProviders.Extensions;
using BeachApi.Swagger;
using FluentValidation;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using OperationResults.AspNetCore;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using TinyHelpers.AspNetCore.Extensions;
using TinyHelpers.AspNetCore.Swagger;
using TinyHelpers.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services, builder.Configuration, builder.Host, builder.Environment);

var app = builder.Build();
Configure(app, app.Environment, app.Services);

await app.RunAsync();

void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostBuilder host, IWebHostEnvironment environment)
{
    var appSettings = services.ConfigureAndGet<AppSettings>(configuration, nameof(AppSettings));
    var jwtSettings = services.ConfigureAndGet<JwtSettings>(configuration, nameof(JwtSettings));

    var swaggerSettings = services.ConfigureAndGet<SwaggerSettings>(configuration, nameof(SwaggerSettings));
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

    services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
            options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

    services.AddEndpointsApiExplorer();

    services.AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    if (swaggerSettings.Enabled)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        services.AddSwaggerGen(options =>
        {
            options.AddAcceptLanguageHeader();
            options.AddDefaultResponse();

            options.OperationFilter<AuthResponseOperationFilter>();
            options.OperationFilter<SwaggerDefaultValues>();

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

            options.MapType<DateTime>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date-time",
                Example = new OpenApiString(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))
            });

            options.MapType<DateOnly>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date",
                Example = new OpenApiString(DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"))
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

            options.CustomOperationIds(api => $"{api.ActionDescriptor.RouteValues["controller"]}_{api.ActionDescriptor.RouteValues["action"]}");
            options.UseAllOfToExtendReferenceSchemas();
        })
        .AddFluentValidationRulesToSwagger(options =>
        {
            options.SetNotNullableIfMinLengthGreaterThenZero = true;
        });
    }

    services.AddAutoMapper(typeof(UserMapperProfile).Assembly);
    services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

    services.AddFluentValidationAutoValidation(options =>
    {
        options.DisableDataAnnotationsValidation = true;
    });

    services.AddFluentEmail(sendinblueSettings.FromEmailAddress).AddSendinblueSender();
    services.AddDataProtection().PersistKeysToDbContext<AuthenticationDbContext>();

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

        options.AddPolicy("Administrator", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(RoleNames.Administrator, RoleNames.PowerUser);
            policy.Requirements.Add(new UserActiveRequirement());
        });

        options.AddPolicy("UserActive", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(RoleNames.User);
            policy.Requirements.Add(new UserActiveRequirement());
        });
    });

    services.AddScoped<IAuthorizationHandler, UserActiveHandler>();
    services.AddScoped<IUserService, HttpUserService>();

    services.AddScoped<ITenantService, TenantService>();
    services.AddSingleton<IJwtTokenService, JwtTokenService>();

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
    services.AddHostedService<UserBackgroundService>();
}

void Configure(IApplicationBuilder app, IWebHostEnvironment environment, IServiceProvider services)
{
    var appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;
    var swaggerSettings = services.GetRequiredService<IOptions<SwaggerSettings>>().Value;

    app.UseHttpsRedirection();
    app.UseRequestLocalization();

    environment.ApplicationName = appSettings.ApplicationName;

    if (swaggerSettings.Enabled)
    {
        var apiVersionDescriptionProvider = services.GetRequiredService<IApiVersionDescriptionProvider>();
        app.UseMiddleware<SwaggerAuthenticationMiddleware>();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName);
                options.RoutePrefix = string.Empty;
            }
        });
    }

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}