using BeachApi.Authentication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeachApi.BusinessLayer.BackgroundServices;

public class UserBackgroundService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;

    public UserBackgroundService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var users = await userManager.Users.ToListAsync(stoppingToken);
            foreach (var user in users)
            {
                if (user.LockoutEnd <= DateTimeOffset.UtcNow)
                {
                    user.LockoutEnd = null;
                    await userManager.UpdateAsync(user);
                }
            }
        }
    }
}