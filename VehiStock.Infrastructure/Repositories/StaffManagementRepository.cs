using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Repositories;

// Implementation for admin staff management data access
public class StaffManagementRepository : IStaffManagementRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public StaffManagementRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(IReadOnlyCollection<ApplicationUser> Users, int TotalRecords)> GetStaffUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users
            .Include(x => x.StaffProfile)
            .Where(x => x.StaffProfile != null)
            .OrderBy(x => x.FullName);

        var totalRecords = await query.CountAsync(cancellationToken);
        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (users, totalRecords);
    }

    public Task<ApplicationUser?> GetUserWithStaffProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _userManager.Users
            .Include(x => x.StaffProfile)
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetRolesAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToArray();
    }

    public async Task UpdateRoleAsync(ApplicationUser user, string role, CancellationToken cancellationToken = default)
    {
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(" ", removeResult.Errors.Select(x => x.Description)));
            }
        }

        var addResult = await _userManager.AddToRoleAsync(user, role);
        if (!addResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", addResult.Errors.Select(x => x.Description)));
        }
    }
}
