using Microsoft.AspNetCore.Identity;
using VehiStock.Domain.Constants;
using VehiStock.Entities;

namespace VehiStock.Domain.Seeders;

public static class AdminSeeder
{
    public static async Task SeedAsync(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        AdminSeedSettings seedSettings)
    {
        if (!await roleManager.RoleExistsAsync(RoleNames.Admin))
        {
            throw new InvalidOperationException($"The '{RoleNames.Admin}' role must be seeded before the admin user is created.");
        }

        if (string.IsNullOrWhiteSpace(seedSettings.Email) || string.IsNullOrWhiteSpace(seedSettings.Password))
        {
            return;
        }

        var adminEmail = seedSettings.Email.Trim();
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                FullName = string.IsNullOrWhiteSpace(seedSettings.FullName)
                    ? "System Administrator"
                    : seedSettings.FullName.Trim(),
                UserName = adminEmail,
                Email = adminEmail,
                PhoneNumber = string.IsNullOrWhiteSpace(seedSettings.PhoneNumber) ? null : seedSettings.PhoneNumber.Trim(),
                ProfilePhotoUrl = string.IsNullOrWhiteSpace(seedSettings.ProfilePhotoUrl) ? null : seedSettings.ProfilePhotoUrl.Trim(),
                EmailConfirmed = true,
                PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(seedSettings.PhoneNumber),
                IsActive = true
            };

            var createUserResult = await userManager.CreateAsync(adminUser, seedSettings.Password);
            if (!createUserResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed admin user: {string.Join(", ", createUserResult.Errors.Select(x => x.Description))}");
            }
        }
        else
        {
            var hasUpdates = false;

            var fullName = string.IsNullOrWhiteSpace(seedSettings.FullName)
                ? "System Administrator"
                : seedSettings.FullName.Trim();

            if (!string.Equals(adminUser.FullName, fullName, StringComparison.Ordinal))
            {
                adminUser.FullName = fullName;
                hasUpdates = true;
            }

            var phoneNumber = string.IsNullOrWhiteSpace(seedSettings.PhoneNumber) ? null : seedSettings.PhoneNumber.Trim();
            if (!string.Equals(adminUser.PhoneNumber, phoneNumber, StringComparison.Ordinal))
            {
                adminUser.PhoneNumber = phoneNumber;
                adminUser.PhoneNumberConfirmed = phoneNumber is not null;
                hasUpdates = true;
            }

            var profilePhotoUrl = string.IsNullOrWhiteSpace(seedSettings.ProfilePhotoUrl) ? null : seedSettings.ProfilePhotoUrl.Trim();
            if (!string.Equals(adminUser.ProfilePhotoUrl, profilePhotoUrl, StringComparison.Ordinal))
            {
                adminUser.ProfilePhotoUrl = profilePhotoUrl;
                hasUpdates = true;
            }

            if (!adminUser.EmailConfirmed)
            {
                adminUser.EmailConfirmed = true;
                hasUpdates = true;
            }

            if (!adminUser.IsActive)
            {
                adminUser.IsActive = true;
                hasUpdates = true;
            }

            if (hasUpdates)
            {
                var updateResult = await userManager.UpdateAsync(adminUser);
                if (!updateResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to update seeded admin user: {string.Join(", ", updateResult.Errors.Select(x => x.Description))}");
                }
            }
        }

        var currentRoles = await userManager.GetRolesAsync(adminUser);
        if (currentRoles.Count > 0 && !currentRoles.Contains(RoleNames.Admin, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                $"The seeded admin email '{adminEmail}' is already assigned to another role: {string.Join(", ", currentRoles)}.");
        }

        if (!await userManager.IsInRoleAsync(adminUser, RoleNames.Admin))
        {
            var addToRoleResult = await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
            if (!addToRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to assign admin role to '{adminEmail}': {string.Join(", ", addToRoleResult.Errors.Select(x => x.Description))}");
            }
        }
    }
}
