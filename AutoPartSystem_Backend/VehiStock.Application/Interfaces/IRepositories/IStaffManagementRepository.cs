using VehiStock.Application.Dtos.Admin;
using VehiStock.Application.Dtos.Management;
using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

// Used for admin staff management data access
public interface IStaffManagementRepository
{
    Task<(IReadOnlyCollection<ApplicationUser> Users, int TotalRecords)> GetStaffUsersAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetUserWithStaffProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetRolesAsync(ApplicationUser user);
    Task UpdateRoleAsync(ApplicationUser user, string role, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<StaffInvoiceActivityResponse>> GetRecentInvoiceActivityAsync(int staffMemberId, int take, CancellationToken cancellationToken = default);
    Task<(int TotalInvoicesCreated, decimal TotalInvoiceValue)> GetInvoiceSummaryAsync(int staffMemberId, CancellationToken cancellationToken = default);
}
