using VehiStock.Application.Dtos.Admin;
using VehiStock.Application.Dtos.Common;

namespace VehiStock.Application.Interfaces.IServices;

// Used for admin staff management
public interface IStaffManagementService
{
    Task<PaginatedResponse<StaffSummaryResponse>> GetStaffAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<StaffSummaryResponse> UpdateRoleAsync(string userId, UpdateStaffRoleRequest request, CancellationToken cancellationToken = default);
}
