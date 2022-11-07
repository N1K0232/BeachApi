using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using BeachApi.Authentication.Common;
using BeachApi.Authentication.Entities;
using BeachApi.Authentication.Extensions;
using BeachApi.Authentication.Settings;
using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.Shared.Requests;
using BeachApi.Shared.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BeachApi.BusinessLayer.Services;

public class UserService : IUserService
{
    private readonly JwtSettings jwtSettings;

    private readonly UserManager<AuthenticationUser> userManager;
    private readonly SignInManager<AuthenticationUser> signInManager;

    private readonly ILogger<UserService> logger;
    private readonly IMemoryCache cache;

    private readonly IMapper mapper;

    public UserService(IOptions<JwtSettings> jwtSettingsOptions,
        UserManager<AuthenticationUser> userManager,
        SignInManager<AuthenticationUser> signInManager,
        ILogger<UserService> logger,
        IMemoryCache cache,
        IMapper mapper)
    {
        jwtSettings = jwtSettingsOptions.Value;

        this.userManager = userManager;
        this.signInManager = signInManager;

        this.logger = logger;
        this.cache = cache;

        this.mapper = mapper;
    }


    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        logger.LogInformation("user login");

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return null;
        }

        var signInResult = await signInManager.PasswordSignInAsync(user, request.Password, false, false);
        if (!signInResult.Succeeded)
        {
            user.AccessFailedCount++;
            await userManager.UpdateAsync(user);

            return null;
        }

        await userManager.UpdateSecurityStampAsync(user);

        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.DateOfBirth, user.DateOfBirth.ToString()),
            new Claim(ClaimTypes.SerialNumber, user.SecurityStamp),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName)
        }.Union(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var loginResponse = CreateResponse(claims);

        user.RefreshToken = loginResponse.RefreshToken;
        user.RefreshTokenExpirationDate = DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes);
        await userManager.UpdateAsync(user);

        return loginResponse;
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        logger.LogInformation("refresh token");

        var user = ValidateAccessToken(request.AccessToken);
        if (user is not null)
        {
            var userId = user.GetId();
            var dbUser = await GetUserAsync(userId);

            if (dbUser?.RefreshToken is null || dbUser?.RefreshTokenExpirationDate < DateTime.UtcNow || dbUser?.RefreshToken != request.RefreshToken)
            {
                return null;
            }

            var loginResponse = CreateResponse(user.Claims);

            dbUser.RefreshToken = loginResponse.RefreshToken;
            dbUser.RefreshTokenExpirationDate = DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes);
            await userManager.UpdateAsync(dbUser);

            return loginResponse;
        }

        return null;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        logger.LogInformation("user registration");

        var user = await CreateUserAsync(request);
        var result = await userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            var roles = new List<string> { RoleNames.User, RoleNames.Customer };
            result = await userManager.AddToRolesAsync(user, roles);
        }

        var response = new RegisterResponse { Succeeded = result.Succeeded, Errors = result.Errors.Select(e => e.Description) };
        return response;
    }


    private async Task<AuthenticationUser> CreateUserAsync(RegisterRequest request)
    {
        var userId = request.Id.GetValueOrDefault(Guid.Empty);
        var user = await GetUserAsync(userId);

        if (user is null)
        {
            user = mapper.Map<AuthenticationUser>(request);
        }
        else
        {
            mapper.Map(request, user);
        }

        return user;
    }

    private async Task<AuthenticationUser> GetUserAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        return user;
    }

    private LoginResponse CreateResponse(IEnumerable<Claim> claims)
    {
        var securityKey = GetSecurityKey();
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(jwtSettings.Issuer, jwtSettings.Audience, claims,
            DateTime.UtcNow, DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes), credentials);

        using var generator = RandomNumberGenerator.Create();
        var randomNumber = new byte[256];
        generator.GetBytes(randomNumber);

        var tokenHandler = new JwtSecurityTokenHandler();

        var accessToken = tokenHandler.WriteToken(jwtSecurityToken);
        var refreshToken = Convert.ToBase64String(randomNumber);

        var loginResponse = new LoginResponse { AccessToken = accessToken, RefreshToken = refreshToken };
        return loginResponse;
    }

    private ClaimsPrincipal ValidateAccessToken(string accessToken)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecurityKey)),
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var user = tokenHandler.ValidateToken(accessToken, parameters, out var securityToken);
            if (securityToken is JwtSecurityToken jwtSecurityToken && jwtSecurityToken.Header.Alg == SecurityAlgorithms.HmacSha256)
            {
                return user;
            }
        }
        catch
        {
        }

        return null;
    }

    private SymmetricSecurityKey GetSecurityKey()
    {
        var bytes = Encoding.UTF8.GetBytes(jwtSettings.SecurityKey);
        return new SymmetricSecurityKey(bytes);
    }
}