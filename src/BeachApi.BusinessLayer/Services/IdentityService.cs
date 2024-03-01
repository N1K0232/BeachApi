using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BeachApi.Authentication;
using BeachApi.Authentication.Entities;
using BeachApi.Authentication.Extensions;
using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.BusinessLayer.Settings;
using BeachApi.Shared.Models.Requests;
using BeachApi.Shared.Models.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OperationResults;

namespace BeachApi.BusinessLayer.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly JwtSettings jwtSettings;

    public IdentityService(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<JwtSettings> jwtSettingsOptions)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;

        jwtSettings = jwtSettingsOptions.Value;
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var signInResult = await signInManager.PasswordSignInAsync(request.UserName, request.Password, false, false);
        if (!signInResult.Succeeded)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid username or password");
        }

        var user = await userManager.FindByNameAsync(request.UserName);
        await userManager.UpdateSecurityStampAsync(user);

        var userRoles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GroupSid, user.TenantId?.ToString() ?? string.Empty),
            new Claim(ClaimTypes.SerialNumber, user.SecurityStamp ?? string.Empty)
        }.Union(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var response = CreateToken(claims);
        await SaveRefreshTokenAsync(user, response.RefreshToken);

        return response;
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = await ValidateAccessTokenAsync(request.AccessToken);
        if (user is not null)
        {
            var dbUser = await userManager.FindByIdAsync(user.GetClaimValue(ClaimTypes.NameIdentifier));
            if (dbUser?.RefreshToken is null || dbUser.RefreshTokenExpirationDate < DateTime.UtcNow || dbUser.RefreshToken != request.RefreshToken)
            {
                return Result.Fail(FailureReasons.ClientError, "Invalid refresh token");
            }

            var response = CreateToken(user.Claims);
            await SaveRefreshTokenAsync(dbUser, response.RefreshToken);

            return response;
        }

        return Result.Fail(FailureReasons.ClientError, "Invalid access token signature", "Couldn't verify the access token");
    }

    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.UserName ?? request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
        {
            result = await userManager.AddToRoleAsync(user, RoleNames.User);
        }

        return result.Succeeded ?
            Result.Ok() :
            Result.Fail(FailureReasons.ClientError, "Registration failed", result.GetErrors());
    }

    private AuthResponse CreateToken(IEnumerable<Claim> claims)
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
        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = GenerateRefreshToken()
        };

        return response;

        static string GenerateRefreshToken()
        {
            using var generator = RandomNumberGenerator.Create();
            var randomNumber = new byte[256];

            generator.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    private async Task SaveRefreshTokenAsync(ApplicationUser user, string refreshToken)
    {
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpirationDate = DateTime.UtcNow.AddMinutes(jwtSettings.RefreshTokenExpirationMinutes);

        await userManager.UpdateAsync(user);
    }

    private Task<ClaimsPrincipal> ValidateAccessTokenAsync(string accessToken)
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