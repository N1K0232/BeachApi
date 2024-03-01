using BeachApi.Authentication;
using BeachApi.Contracts;
using BeachApi.MultiTenant;
using Microsoft.Extensions.Caching.Memory;

namespace BeachApi.Services;

public class TenantService : ITenantService
{
    private readonly AuthenticationDbContext authenticationDbContext;
    private readonly IUserService userService;
    private readonly IMemoryCache cache;

    public TenantService(AuthenticationDbContext authenticationDbContext, IUserService userService,
        IMemoryCache cache)
    {
        this.authenticationDbContext = authenticationDbContext;
        this.userService = userService;
        this.cache = cache;
    }

    public Tenant Get()
    {
        var tenants = cache.GetOrCreate("tenants", entry =>
        {
            var tenants = authenticationDbContext.Tenants
                .ToDictionary(k => k.Id, v => new Tenant(v.Id, v.ConnectionString, v.StorageConnectionString, v.ContainerName));

            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return tenants;
        });

        var tenantId = userService.GetTenantId();
        if (tenants.TryGetValue(tenantId, out var tenant))
        {
            return tenant;
        }

        return tenant;
    }
}