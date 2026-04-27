using VehiStock.Application.DTOs.Staff;
using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

// Used for auth management and staff data access
public interface IStaffAdministrationRepository
{
    Task EnsureRoleExistsAsync(string roleName);
    Task<ApplicationUser?> FindUserByEmailAsync(string email);
    Task<bool> StaffCodeExistsAsync(string staffCode, CancellationToken cancellationToken = default);
    Task<IdentityOperationResult> CreateUserAsync(ApplicationUser user, string password);
    Task<IdentityOperationResult> DeleteUserAsync(ApplicationUser user);
    Task<IdentityOperationResult> AddUserToRoleAsync(ApplicationUser user, string roleName);
    Task<IdentityOperationResult> RemoveUserFromRolesAsync(ApplicationUser user, IEnumerable<string> roleNames);
    Task<IReadOnlyList<string>> GetUserRolesAsync(ApplicationUser user);
    Task AddStaffProfileAsync(StaffProfile staffProfile, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StaffProfile>> GetStaffProfilesAsync(CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetStaffUserByIdAsync(string userId, CancellationToken cancellationToken = default);
}
