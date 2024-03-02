using System.Security.Claims;

namespace BeachApi.Contracts;

public interface IJwtTokenService
{
    string GenerateAccessToken(IEnumerable<Claim> claims);

    string GenerateRefreshToken(out DateTime expirationDate);

    Task<ClaimsPrincipal> ValidateTokenAsync(string accessToken);
}