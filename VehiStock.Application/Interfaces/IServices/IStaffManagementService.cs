using VehiStock.Application.Dtos.Admin;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Management;

namespace VehiStock.Application.Interfaces.IServices;

// Used for admin staff management
public interface IStaffManagementService
{
    Task<PaginatedResponse<StaffSummaryResponse>> GetStaffAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<StaffDetailResponse> GetStaffDetailAsync(string userId, CancellationToken cancellationToken = default);
    Task<StaffSummaryResponse> UpdateRoleAsync(string userId, UpdateStaffRoleRequest request, CancellationToken cancellationToken = default);
}
