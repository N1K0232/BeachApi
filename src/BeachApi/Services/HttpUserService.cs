using System.Security.Claims;
using BeachApi.Authentication.Extensions;
using BeachApi.Contracts;

namespace BeachApi.Services;

public class HttpUserService : IUserService
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public HttpUserService(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public Guid GetId() => httpContextAccessor.HttpContext.User.GetId();

    public Guid GetTenantId()
    {
        var value = httpContextAccessor.HttpContext.User.GetClaimValue(ClaimTypes.GroupSid);
        if (Guid.TryParse(value, out var tenantId))
        {
            return tenantId;
        }

        return Guid.Empty;
    }

    public string GetUserName() => httpContextAccessor.HttpContext.User.GetUserName();
}