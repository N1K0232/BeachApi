using BeachApi.Authentication;
using BeachApi.Authentication.Entities;
using BeachApi.Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BeachApi.BusinessLayer.StartupServices;

public class IdentityUserStartupService : IHostedService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IOptions<AdministratorUser> administratorUserOptions;
    private readonly IOptions<PowerUser> powerUserOptions;

    public IdentityUserStartupService(IServiceProvider serviceProvider,
        IOptions<AdministratorUser> administratorUserOptions,
        IOptions<PowerUser> powerUserOptions)
    {
        this.serviceProvider = serviceProvider;
        this.administratorUserOptions = administratorUserOptions;
        this.powerUserOptions = powerUserOptions;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var administratorUser = new ApplicationUser
        {
            FirstName = administratorUserOptions.Value.FirstName,
            Email = administratorUserOptions.Value.Email,
            UserName = administratorUserOptions.Value.Email
        };

        var powerUser = new ApplicationUser
        {
            FirstName = powerUserOptions.Value.FirstName,
            Email = powerUserOptions.Value.Email,
            UserName = powerUserOptions.Value.Email
        };

        await CheckCreateUserAsync(administratorUser, administratorUserOptions.Value.Password, RoleNames.Administrator, RoleNames.User);
        await CheckCreateUserAsync(powerUser, powerUserOptions.Value.Password, RoleNames.PowerUser, RoleNames.User);
    }

    private async Task CheckCreateUserAsync(ApplicationUser user, string password, params string[] roles)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var dbUser = await userManager.FindByEmailAsync(user.Email);
        if (dbUser is null)
        {
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRolesAsync(user, roles);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var users = await userManager.Users.ToListAsync(cancellationToken);
        foreach (var user in users)
        {
            var lockedOut = await userManager.IsLockedOutAsync(user);
            if (lockedOut)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpirationDate = null;

                await userManager.UpdateAsync(user);
            }
        }
    }
}