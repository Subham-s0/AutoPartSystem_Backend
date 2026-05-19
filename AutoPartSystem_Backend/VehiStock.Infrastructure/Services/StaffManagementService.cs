using VehiStock.Application.Dtos.Admin;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Management;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

// Implementation for admin staff management
public class StaffManagementService : IStaffManagementService
{
    private static readonly HashSet<string> AllowedRoles = [RoleNames.Admin, RoleNames.Staff];

    private readonly IStaffManagementRepository _staffManagementRepository;

    public StaffManagementService(IStaffManagementRepository staffManagementRepository)
    {
        _staffManagementRepository = staffManagementRepository;
    }

    public async Task<PaginatedResponse<StaffSummaryResponse>> GetStaffAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedPageNumber = Math.Max(1, pageNumber);
        var normalizedPageSize = Math.Clamp(pageSize, 1, 100);
        var (users, totalRecords) = await _staffManagementRepository.GetStaffUsersAsync(search, normalizedPageNumber, normalizedPageSize, cancellationToken);

        var items = new List<StaffSummaryResponse>(users.Count);
        foreach (var user in users)
        {
            items.Add(await MapAsync(user));
        }

        return new PaginatedResponse<StaffSummaryResponse>
        {
            Items = items,
            PageNumber = normalizedPageNumber,
            PageSize = normalizedPageSize,
            TotalRecords = totalRecords,
            TotalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)normalizedPageSize)
        };
    }

    public async Task<StaffDetailResponse> GetStaffDetailAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _staffManagementRepository.GetUserWithStaffProfileAsync(userId, cancellationToken);
        if (user is null || user.StaffProfile is null)
        {
            throw new InvalidOperationException("Staff user was not found.");
        }

        var roles = await _staffManagementRepository.GetRolesAsync(user);
        var recentInvoices = await _staffManagementRepository.GetRecentInvoiceActivityAsync(user.StaffProfile.StaffMemberId, 10, cancellationToken);
        var invoiceSummary = await _staffManagementRepository.GetInvoiceSummaryAsync(user.StaffProfile.StaffMemberId, cancellationToken);

        return new StaffDetailResponse
        {
            UserId = user.Id,
            StaffMemberId = user.StaffProfile.StaffMemberId,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            IsActive = user.IsActive,
            Role = roles.FirstOrDefault() ?? string.Empty,
            JobTitle = user.StaffProfile.JobTitle,
            HireDate = user.StaffProfile.HireDate,
            TotalInvoicesCreated = invoiceSummary.TotalInvoicesCreated,
            TotalInvoiceValue = invoiceSummary.TotalInvoiceValue,
            RecentInvoices = recentInvoices
        };
    }

    public async Task<StaffSummaryResponse> UpdateRoleAsync(string userId, UpdateStaffRoleRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _staffManagementRepository.GetUserWithStaffProfileAsync(userId, cancellationToken);
        if (user is null || user.StaffProfile is null)
        {
            throw new InvalidOperationException("Staff user was not found.");
        }

        var normalizedRole = request.Role.Trim();
        if (!AllowedRoles.Contains(normalizedRole))
        {
            throw new InvalidOperationException("Role must be Admin or Staff.");
        }

        await _staffManagementRepository.UpdateRoleAsync(user, normalizedRole, cancellationToken);
        return await MapAsync(user);
    }

    private async Task<StaffSummaryResponse> MapAsync(ApplicationUser user)
    {
        var roles = await _staffManagementRepository.GetRolesAsync(user);
        return new StaffSummaryResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            JobTitle = user.StaffProfile?.JobTitle ?? string.Empty,
            HireDate = user.StaffProfile?.HireDate ?? default,
            Role = roles.FirstOrDefault() ?? string.Empty,
            IsActive = user.IsActive,
            StaffMemberId = user.StaffProfile?.StaffMemberId ?? 0
        };
    }
}
