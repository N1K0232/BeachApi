﻿using System.Security.Claims;
using System.Security.Principal;

namespace BeachApi.Authentication.Extensions;

public static class ClaimsExtensions
{
    public static Guid GetId(this IPrincipal user)
    {
        var value = GetClaimValue(user, ClaimTypes.NameIdentifier);
        if (Guid.TryParse(value, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }

    public static string GetFirstName(this IPrincipal user) => GetClaimValue(user, ClaimTypes.GivenName);

    public static string GetLastName(this IPrincipal user) => GetClaimValue(user, ClaimTypes.Surname);

    public static DateTime GetDateOfBirth(this IPrincipal user)
    {
        var value = GetClaimValue(user, ClaimTypes.DateOfBirth);
        if (DateTime.TryParse(value, out var dateOfBirth))
        {
            return dateOfBirth;
        }

        return DateTime.MinValue;
    }

    public static string GetUserName(this IPrincipal user) => GetClaimValue(user, ClaimTypes.Name);

    public static string GetEmail(this IPrincipal user) => GetClaimValue(user, ClaimTypes.Email);

    public static string GetPhoneNumber(this IPrincipal user) => GetClaimValue(user, ClaimTypes.MobilePhone);

    public static string GetClaimValue(this IPrincipal user, string claimType)
    {
        if (!user.Identity.IsAuthenticated)
        {
            return null;
        }

        var principal = (ClaimsPrincipal)user;
        var claim = principal.FindFirst(claimType);

        return claim?.Value ?? string.Empty;
    }
}