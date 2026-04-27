using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehiStock.Application.DTOs.Staff;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

// Implementation for auth management and staff data access
public class StaffAdministrationRepository(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager) : IStaffAdministrationRepository
{
    public async Task EnsureRoleExistsAsync(string roleName)
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

    public Task<ApplicationUser?> FindUserByEmailAsync(string email) => userManager.FindByEmailAsync(email);

    public Task<bool> StaffCodeExistsAsync(string staffCode, CancellationToken cancellationToken = default) =>
        dbContext.StaffProfiles.AnyAsync(x => x.StaffCode == staffCode, cancellationToken);

    public async Task<IdentityOperationResult> CreateUserAsync(ApplicationUser user, string password)
    {
        var result = await userManager.CreateAsync(user, password);
        return MapResult(result);
    }

    public async Task<IdentityOperationResult> DeleteUserAsync(ApplicationUser user)
    {
        var result = await userManager.DeleteAsync(user);
        return MapResult(result);
    }

    public async Task<IdentityOperationResult> AddUserToRoleAsync(ApplicationUser user, string roleName)
    {
        var result = await userManager.AddToRoleAsync(user, roleName);
        return MapResult(result);
    }

    public async Task<IdentityOperationResult> RemoveUserFromRolesAsync(ApplicationUser user, IEnumerable<string> roleNames)
    {
        var result = await userManager.RemoveFromRolesAsync(user, roleNames);
        return MapResult(result);
    }

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(ApplicationUser user) =>
        (await userManager.GetRolesAsync(user)).ToArray();

    public async Task AddStaffProfileAsync(StaffProfile staffProfile, CancellationToken cancellationToken = default)
    {
        dbContext.StaffProfiles.Add(staffProfile);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StaffProfile>> GetStaffProfilesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.StaffProfiles
            .Include(x => x.User)
            .OrderBy(x => x.StaffCode)
            .ToListAsync(cancellationToken);

    public Task<ApplicationUser?> GetStaffUserByIdAsync(string userId, CancellationToken cancellationToken = default) =>
        userManager.Users
            .Include(x => x.StaffProfile)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

    private static IdentityOperationResult MapResult(IdentityResult result) =>
        new()
        {
            Succeeded = result.Succeeded,
            Errors = result.Errors.Select(x => x.Description).ToArray()
        };
}
