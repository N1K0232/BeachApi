using BeachApi.Authentication.Extensions;
using BeachApi.SharedServices;
using Microsoft.AspNetCore.Http;

namespace BeachApi.Authentication.InternalServices;

internal class UserClaimService : IUserClaimService
{
    private readonly HttpContext httpContext;

    public UserClaimService(IHttpContextAccessor httpContextAccessor)
    {
        httpContext = httpContextAccessor.HttpContext;
    }

    public Guid GetId() => httpContext.User.GetId();

    public string GetUserName() => httpContext.User.GetUserName();
}