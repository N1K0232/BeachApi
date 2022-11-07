using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BeachApi.Authentication;
using BeachApi.Authentication.Entities;
using BeachApi.Authentication.Settings;
using BeachApi.Authentication.StartupTasks;
using BeachApi.Authorization.Handlers;
using BeachApi.Authorization.Requirements;
using BeachApi.BusinessLayer.Services;
using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.Documentation;
using BeachApi.Security;
using Hellang.Middleware.ProblemDetails;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OperationResults.AspNetCore;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using TinyHelpers.Json.Serialization;

namespace BeachApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
        });

        builder.Services.AddProblemDetails();
        builder.Services.AddMemoryCache();
        builder.Services.AddOperationResult();
        builder.Services.AddUserClaimService();

        builder.Services.AddMapperProfiles();
        builder.Services.AddFluentValidation();

        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
            options.JsonSerializerOptions.Converters.Add(new TimeSpanTicksConverter());
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
        });
        builder.Services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        builder.Services.AddSwaggerGen(options =>
        {
            options.OperationFilter<SwaggerDefaultValues>();

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Insert JWT token with the \"Bearer \" prefix",
                Name = "Authorization",
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

        var hashedConnectionString = builder.Configuration.GetConnectionString("SqlConnection");
        var connectionString = StringHasher.GetString(hashedConnectionString);

        builder.Services.AddSqlServer<AuthenticationDataContext>(connectionString);
        builder.Services.AddIdentity<AuthenticationUser, AuthenticationRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireDigit = true;
        })
        .AddEntityFrameworkStores<AuthenticationDataContext>()
        .AddDefaultTokenProviders();

        var jwtSection = builder.Configuration.GetSection(nameof(JwtSettings));
        var jwtSettings = jwtSection.Get<JwtSettings>();
        builder.Services.Configure<JwtSettings>(jwtSection);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidAudience = jwtSettings.Audience,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(jwtSettings.SecurityKey)),
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddAuthorization(options =>
        {
            var policyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
            policyBuilder.Requirements.Add(new UserActiveRequirement());

            var policy = policyBuilder.Build();
            options.FallbackPolicy = options.DefaultPolicy = policy;

            options.AddPolicy("UserActive", policy =>
            {
                policy.Requirements.Add(new UserActiveRequirement());
            });
        });

        builder.Services.AddScoped<IAuthorizationHandler, UserActiveHandler>();

        builder.Services.AddHealthChecks().AddAsyncCheck("sql", async () =>
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message, ex);
            }

            return HealthCheckResult.Healthy();
        });

        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddHostedService<AuthenticationStartupTask>();


        var app = builder.Build();

        app.UseProblemDetails();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName);
                options.RoutePrefix = string.Empty;
            }
        });

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSerilogRequestLogging(options =>
        {
            options.IncludeQueryInRequestPath = true;
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();

            endpoints.MapHealthChecks("/status",
                new HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        var result = JsonSerializer.Serialize(
                            new
                            {
                                Status = report.Status.ToString(),
                                Details = report.Entries.Select(e => new
                                {
                                    Service = e.Key,
                                    Status = Enum.GetName(typeof(HealthStatus), e.Value.Status),
                                    Description = e.Value.Description
                                })
                            });

                        context.Response.ContentType = MediaTypeNames.Application.Json;
                        await context.Response.WriteAsync(result);
                    }
                });
        });

        app.Run();
    }
}