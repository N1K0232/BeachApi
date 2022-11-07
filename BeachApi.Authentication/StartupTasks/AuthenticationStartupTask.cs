using BeachApi.Authentication.Common;
using BeachApi.Authentication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeachApi.Authentication.StartupTasks;

public class AuthenticationStartupTask : IHostedService
{
    private readonly IServiceProvider serviceProvider;

    public AuthenticationStartupTask(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        using var roleManager = services.GetRequiredService<RoleManager<AuthenticationRole>>();

        var roleNames = new string[] { RoleNames.Administrator, RoleNames.PowerUser, RoleNames.Staff, RoleNames.User, RoleNames.Customer };

        foreach (var roleName in roleNames)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await roleManager.CreateAsync(new AuthenticationRole(roleName));
            }
        }

        using var userManager = services.GetRequiredService<UserManager<AuthenticationUser>>();

        var user = new AuthenticationUser
        {
            FirstName = "Nicola",
            LastName = "Silvestri",
            DateOfBirth = DateTime.Parse("22/10/2002"),
            Email = "ns.nicolasilvestri@gmail.com",
            UserName = "N1K0232",
            PhoneNumber = "331 990 7702"
        };

        await RegisterAsync(user, "NicoSilve22!", RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User);

        async Task RegisterAsync(AuthenticationUser administratorUser, string password, params string[] roles)
        {
            var dbUser = await userManager.FindByNameAsync(administratorUser.UserName);
            if (dbUser == null)
            {
                var result = await userManager.CreateAsync(administratorUser, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(user, roles);
                }
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}