using BeachApi.BusinessLayer.RemoteServices;
using FluentEmail.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BeachApi.Extensions;

public static class FluentEmailBuilderExtensions
{
    public static FluentEmailServicesBuilder AddSendinblueSender(this FluentEmailServicesBuilder builder)
    {
        builder.Services.TryAddSingleton<ISender, SendinblueSender>();
        return builder;
    }
}