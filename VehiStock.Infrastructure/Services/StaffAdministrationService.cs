using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Common;
using VehiStock.Application.DTOs.Staff;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Services;

public class StaffAdministrationService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager) : IStaffAdministrationService
{
    public async Task<StaffSummaryDto> RegisterStaffAsync(RegisterStaffRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedRole = NormalizeRole(request.Role);
        await EnsureRoleExistsAsync(normalizedRole);

        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var existingStaffCode = await dbContext.StaffProfiles
            .AnyAsync(x => x.StaffCode == request.StaffCode, cancellationToken);

        if (existingStaffCode)
        {
            throw new InvalidOperationException("A staff member with this staff code already exists.");
        }

        var user = new ApplicationUser
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            UserName = request.Email.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            IsActive = true
        };

        var identityResult = await userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", identityResult.Errors.Select(x => x.Description)));
        }

        try
        {
            var addToRoleResult = await userManager.AddToRoleAsync(user, normalizedRole);
            if (!addToRoleResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(" ", addToRoleResult.Errors.Select(x => x.Description)));
            }

            var staffProfile = new StaffProfile
            {
                UserId = user.Id,
                StaffCode = request.StaffCode.Trim(),
                JobTitle = request.JobTitle.Trim(),
                HireDate = request.HireDate
            };

            dbContext.StaffProfiles.Add(staffProfile);
            await dbContext.SaveChangesAsync(cancellationToken);

            return await MapStaffSummaryAsync(user, staffProfile, cancellationToken);
        }
        catch
        {
            await userManager.DeleteAsync(user);
            throw;
        }
    }

    public async Task<IReadOnlyList<StaffSummaryDto>> GetStaffAsync(CancellationToken cancellationToken = default)
    {
        var staffProfiles = await dbContext.StaffProfiles
            .Include(x => x.User)
            .OrderBy(x => x.StaffCode)
            .ToListAsync(cancellationToken);

        var staff = new List<StaffSummaryDto>(staffProfiles.Count);

        foreach (var profile in staffProfiles)
        {
            staff.Add(await MapStaffSummaryAsync(profile.User, profile, cancellationToken));
        }

        return staff;
    }

    public async Task<StaffSummaryDto> UpdateRoleAsync(string userId, UpdateUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedRole = NormalizeRole(request.Role);
        await EnsureRoleExistsAsync(normalizedRole);

        var user = await userManager.Users
            .Include(x => x.StaffProfile)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Staff user was not found.");

        if (user.StaffProfile is null)
        {
            throw new InvalidOperationException("The selected user does not have a staff profile.");
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(" ", removeResult.Errors.Select(x => x.Description)));
            }
        }

        var addResult = await userManager.AddToRoleAsync(user, normalizedRole);
        if (!addResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", addResult.Errors.Select(x => x.Description)));
        }

        return await MapStaffSummaryAsync(user, user.StaffProfile, cancellationToken);
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await roleManager.CreateAsync(new ApplicationRole
        {
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant()
        });

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(x => x.Description)));
        }
    }

    private static string NormalizeRole(string role)
    {
        var matchedRole = RoleNames.All.FirstOrDefault(x => x.Equals(role?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (matchedRole is null)
        {
            throw new InvalidOperationException("Unsupported role.");
        }

        return matchedRole;
    }

    private async Task<StaffSummaryDto> MapStaffSummaryAsync(ApplicationUser user, StaffProfile staffProfile, CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);

        return new StaffSummaryDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            StaffCode = staffProfile.StaffCode,
            JobTitle = staffProfile.JobTitle,
            HireDate = staffProfile.HireDate,
            Role = roles.FirstOrDefault() ?? string.Empty,
            IsActive = user.IsActive
        };
    }
}
