using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VehiStock.Application.Common;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

// Used for role seed management
public static class IdentitySeedService
{
    public static async Task EnsureRolesCreatedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        foreach (var roleName in RoleNames.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            await roleManager.CreateAsync(new ApplicationRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            });
        }
    }
}
