using VehiStock.Application.DTOs.Staff;

namespace VehiStock.Application.Interfaces.IServices;

// Used for auth management
public interface IStaffAdministrationService
{
    Task<StaffSummaryDto> RegisterStaffAsync(RegisterStaffRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StaffSummaryDto>> GetStaffAsync(CancellationToken cancellationToken = default);
    Task<StaffSummaryDto> UpdateRoleAsync(string userId, UpdateUserRoleRequest request, CancellationToken cancellationToken = default);
}
