using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BeachApi.Swagger;

public class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;
        operation.Deprecated = apiDescription.IsDeprecated();

        if (operation.Parameters == null)
        {
            return;
        }

        foreach (var parameter in operation.Parameters)
        {
            var descriptions = apiDescription.ParameterDescriptions;
            var description = descriptions?.FirstOrDefault(p => p.Name == parameter.Name);

            parameter.Description ??= description?.ModelMetadata.Description ?? string.Empty;
        }
    }
}