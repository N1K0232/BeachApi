using BeachApi.Documentation;
using BeachApi.Security;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OperationResults.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using TinyHelpers.Json.Serialization;

namespace BeachApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddProblemDetails();
        builder.Services.AddMemoryCache();
        builder.Services.AddOperationResult();

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
        });

        var hashedConnectionString = builder.Configuration.GetConnectionString("SqlConnection");
        var connectionString = StringHasher.GetString(hashedConnectionString);

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