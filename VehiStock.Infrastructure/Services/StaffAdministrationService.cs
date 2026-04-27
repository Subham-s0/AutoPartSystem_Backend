using VehiStock.Application.Common;
using VehiStock.Application.DTOs.Staff;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

// Implementation for auth management
public class StaffAdministrationService(
    IStaffAdministrationRepository staffAdministrationRepository) : IStaffAdministrationService
{
    public async Task<StaffSummaryDto> RegisterStaffAsync(RegisterStaffRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedRole = NormalizeRole(request.Role);
        await staffAdministrationRepository.EnsureRoleExistsAsync(normalizedRole);

        var existingUser = await staffAdministrationRepository.FindUserByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        if (await staffAdministrationRepository.StaffCodeExistsAsync(request.StaffCode, cancellationToken))
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

        var identityResult = await staffAdministrationRepository.CreateUserAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", identityResult.Errors));
        }

        try
        {
            var addToRoleResult = await staffAdministrationRepository.AddUserToRoleAsync(user, normalizedRole);
            if (!addToRoleResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(" ", addToRoleResult.Errors));
            }

            var staffProfile = new StaffProfile
            {
                UserId = user.Id,
                StaffCode = request.StaffCode.Trim(),
                JobTitle = request.JobTitle.Trim(),
                HireDate = request.HireDate
            };

            await staffAdministrationRepository.AddStaffProfileAsync(staffProfile, cancellationToken);

            return await MapStaffSummaryAsync(user, staffProfile, cancellationToken);
        }
        catch
        {
            await staffAdministrationRepository.DeleteUserAsync(user);
            throw;
        }
    }

    public async Task<IReadOnlyList<StaffSummaryDto>> GetStaffAsync(CancellationToken cancellationToken = default)
    {
        var staffProfiles = await staffAdministrationRepository.GetStaffProfilesAsync(cancellationToken);

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
        await staffAdministrationRepository.EnsureRoleExistsAsync(normalizedRole);

        var user = await staffAdministrationRepository.GetStaffUserByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("Staff user was not found.");

        if (user.StaffProfile is null)
        {
            throw new InvalidOperationException("The selected user does not have a staff profile.");
        }

        var currentRoles = await staffAdministrationRepository.GetUserRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            var removeResult = await staffAdministrationRepository.RemoveUserFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(" ", removeResult.Errors));
            }
        }

        var addResult = await staffAdministrationRepository.AddUserToRoleAsync(user, normalizedRole);
        if (!addResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", addResult.Errors));
        }

        return await MapStaffSummaryAsync(user, user.StaffProfile, cancellationToken);
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
        var roles = await staffAdministrationRepository.GetUserRolesAsync(user);

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
