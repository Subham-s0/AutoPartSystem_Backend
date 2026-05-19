using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Dtos.Staff;

namespace VehiStock.Application.Interfaces.IServices;

public interface ICustomerProfileService
{
    Task<CustomerProfileResponse> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<StaffCustomerResponse>> GetCustomersForStaffAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<StaffCustomerHistoryResponse> GetCustomerHistoryAsync(int customerId, CancellationToken cancellationToken = default);
}
