using Microsoft.AspNetCore.Identity;

namespace BeachApi.Authentication.Extensions;

public static class IdentityExtensions
{
    public static string GetErrors(this IdentityResult result)
        => string.Join(",", result.Errors.Select(e => e.Description));
}