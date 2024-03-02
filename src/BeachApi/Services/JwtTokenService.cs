using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BeachApi.BusinessLayer.Settings;
using BeachApi.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BeachApi.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> jwtSettingsOptions)
    {
        jwtSettings = jwtSettingsOptions.Value;
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var securityKey = Encoding.UTF8.GetBytes(jwtSettings.SecurityKey);
        var symmetricSecurityKey = new SymmetricSecurityKey(securityKey);

        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        var jwtSecurityToken = new JwtSecurityToken
        (
            jwtSettings.Issuer,
            jwtSettings.Audience,
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        return accessToken;
    }

    public string GenerateRefreshToken(out DateTime expirationDate)
    {
        using var generator = RandomNumberGenerator.Create();
        var randomNumber = new byte[256];

        generator.GetBytes(randomNumber);
        expirationDate = DateTime.UtcNow.AddMinutes(jwtSettings.RefreshTokenExpirationMinutes);

        return Convert.ToBase64String(randomNumber);
    }

    public Task<ClaimsPrincipal> ValidateTokenAsync(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecurityKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = false,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var user = handler.ValidateToken(accessToken, parameters, out var securityToken);
            if (securityToken is JwtSecurityToken jwtSecurityToken && jwtSecurityToken.Header.Alg == SecurityAlgorithms.HmacSha256)
            {
                return Task.FromResult(user);
            }
        }
        catch
        {
        }

        return Task.FromResult<ClaimsPrincipal>(null);
    }
}