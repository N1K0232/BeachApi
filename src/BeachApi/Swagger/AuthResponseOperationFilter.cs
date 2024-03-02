﻿using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BeachApi.Swagger;

public class AuthResponseOperationFilter : IOperationFilter
{
    private readonly IAuthorizationPolicyProvider authorizationPolicyProvider;

    public AuthResponseOperationFilter(IAuthorizationPolicyProvider authorizationPolicyProvider)
    {
        this.authorizationPolicyProvider = authorizationPolicyProvider;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fallbackPolicy = authorizationPolicyProvider.GetFallbackPolicyAsync().GetAwaiter().GetResult();
        var requireAuthenticatedUser = fallbackPolicy?.Requirements.Any(r => r is DenyAnonymousAuthorizationRequirement) ?? false;

        var requireAuthorization = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .Any(a => a is AuthorizeAttribute) ?? false;

        var allowAnonymous = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .Any(a => a is AllowAnonymousAttribute) ?? false;

        if ((requireAuthenticatedUser || requireAuthorization) && !allowAnonymous)
        {
            operation.Responses.TryAdd(StatusCodes.Status401Unauthorized.ToString(), GetResponse(HttpStatusCode.Unauthorized.ToString()));
            operation.Responses.TryAdd(StatusCodes.Status403Forbidden.ToString(), GetResponse(HttpStatusCode.Forbidden.ToString()));
        }
    }

    private static OpenApiResponse GetResponse(string description)
    {
        var reference = new OpenApiReference { Id = nameof(ProblemDetails), Type = ReferenceType.Schema };
        var schema = new OpenApiSchema { Reference = reference };

        var jsonMediaType = MediaTypeNames.Application.Json;
        var apiMediaType = new OpenApiMediaType { Schema = schema };
        var content = new Dictionary<string, OpenApiMediaType> { [jsonMediaType] = apiMediaType };

        var response = new OpenApiResponse { Content = content, Description = description };
        return response;
    }
}