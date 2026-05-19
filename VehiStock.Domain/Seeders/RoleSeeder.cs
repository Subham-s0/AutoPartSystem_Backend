using Microsoft.AspNetCore.Identity;
using VehiStock.Domain.Constants;
using VehiStock.Entities;

namespace VehiStock.Domain.Seeders;

public static class RoleSeeder
{
    public static async Task SeedAsync(RoleManager<ApplicationRole> roleManager)
    {
        foreach (var roleName in RoleNames.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var roleResult = await roleManager.CreateAsync(new ApplicationRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            });

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed role '{roleName}': {string.Join(", ", roleResult.Errors.Select(x => x.Description))}");
            }
        }
    }
}
