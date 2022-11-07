using BeachApi.Authentication.InternalServices;
using BeachApi.SharedServices;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserClaimService(this IServiceCollection services)
    {
        services.AddScoped<IUserClaimService, UserClaimService>();
        return services;
    }
}