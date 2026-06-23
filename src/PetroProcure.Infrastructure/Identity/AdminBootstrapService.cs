using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.Infrastructure.Identity;

public sealed class AdminBootstrapService(
    IServiceProvider services,
    IConfiguration configuration,
    IHostEnvironment environment) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var enabled = configuration.GetValue<bool?>("Security:BootstrapAdmin:Enabled")
            ?? environment.IsDevelopment();
        if (!enabled)
            return;

        var password = configuration["Security:BootstrapAdmin:Password"];
        if (string.IsNullOrWhiteSpace(password))
        {
            if (environment.IsProduction())
                throw new InvalidOperationException(
                    "Security:BootstrapAdmin:Password is required when production admin bootstrap is enabled.");
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(IdentitySeedData.DefaultAdminUserId.ToString());
        if (user is null || await userManager.HasPasswordAsync(user))
            return;

        var result = await userManager.AddPasswordAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Admin bootstrap failed: {string.Join("; ", result.Errors.Select(error => error.Description))}");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
