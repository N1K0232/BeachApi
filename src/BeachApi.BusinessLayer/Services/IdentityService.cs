using System.Security.Claims;
using BeachApi.Authentication;
using BeachApi.Authentication.Entities;
using BeachApi.Authentication.Extensions;
using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.Contracts;
using BeachApi.Shared.Models.Requests;
using BeachApi.Shared.Models.Responses;
using Microsoft.AspNetCore.Identity;
using OperationResults;

namespace BeachApi.BusinessLayer.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly IJwtTokenService jwtTokenService;

    public IdentityService(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.jwtTokenService = jwtTokenService;
    }

    public async Task<Result> DeleteAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is not null)
        {
            var userRoles = await userManager.GetRolesAsync(user);
            if (userRoles.Contains(RoleNames.Administrator) || userRoles.Contains(RoleNames.PowerUser))
            {
                return Result.Fail(FailureReasons.ClientError, "Can't delete account", "You can't delete an administrator or a power user");
            }

            await userManager.RemoveFromRolesAsync(user, userRoles);
            await userManager.DeleteAsync(user);

            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No user found");
    }

    public async Task<Result> LockoutAsync(LockoutRequest request)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is not null)
        {
            var userRoles = await userManager.GetRolesAsync(user);
            if (userRoles.Contains(RoleNames.Administrator) || userRoles.Contains(RoleNames.PowerUser))
            {
                return Result.Fail(FailureReasons.ClientError, "Unable to lockout", "You can't lockout an administrator or a power user");
            }

            user.RefreshToken = null;
            user.RefreshTokenExpirationDate = null;

            var result = await userManager.SetLockoutEndDateAsync(user, request.LockoutEnd);
            return result.Succeeded ? Result.Ok() : Result.Fail(FailureReasons.ClientError, "couldn't lockout the user", result.GetErrors());
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No user found");
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.UserName);
        var signInResult = await signInManager.PasswordSignInAsync(user, request.Password, false, false);

        if (signInResult.Succeeded)
        {
            var userRoles = await userManager.GetRolesAsync(user);
            await userManager.UpdateSecurityStampAsync(user);

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

            var response = new AuthResponse
            {
                AccessToken = jwtTokenService.GenerateAccessToken(claims),
                RefreshToken = jwtTokenService.GenerateRefreshToken(out var expirationDate)
            };

            await SaveRefreshTokenAsync(user, response.RefreshToken, expirationDate);
            return response;
        }

        if (signInResult.IsLockedOut)
        {
            return Result.Fail(FailureReasons.ClientError, "you're locked out", $"You're locked out until {user.LockoutEnd}");
        }

        return Result.Fail(FailureReasons.ClientError, "Invalid username or password");
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = await jwtTokenService.ValidateTokenAsync(request.AccessToken);
        if (user is not null)
        {
            var dbUser = await userManager.FindByIdAsync(user.GetClaimValue(ClaimTypes.NameIdentifier));
            if (dbUser?.RefreshToken is null || dbUser.RefreshTokenExpirationDate < DateTime.UtcNow || dbUser.RefreshToken != request.RefreshToken)
            {
                return Result.Fail(FailureReasons.ClientError, "Invalid refresh token");
            }

            var response = new AuthResponse
            {
                AccessToken = jwtTokenService.GenerateAccessToken(user.Claims),
                RefreshToken = jwtTokenService.GenerateRefreshToken(out var expirationDate)
            };

            await SaveRefreshTokenAsync(dbUser, response.RefreshToken, expirationDate);
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

        return result.Succeeded ? Result.Ok() : Result.Fail(FailureReasons.ClientError, "Registration failed", result.GetErrors());
    }

    private async Task SaveRefreshTokenAsync(ApplicationUser user, string refreshToken, DateTime expirationDate)
    {
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpirationDate = expirationDate;

        await userManager.UpdateAsync(user);
    }
}