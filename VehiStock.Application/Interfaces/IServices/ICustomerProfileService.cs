using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Dtos.Management;

namespace VehiStock.Application.Interfaces.IServices;

public interface ICustomerProfileService
{
    Task<CustomerProfileResponse> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<StaffCustomerResponse>> GetCustomersForStaffAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<CustomerDirectoryItemResponse>> GetCustomersAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<CustomerDirectoryDetailResponse> GetCustomerDetailAsync(int customerId, CancellationToken cancellationToken = default);
    Task<VehiStock.Application.Dtos.Management.StaffCustomerHistoryResponse> GetCustomerHistoryAsync(int customerId, CancellationToken cancellationToken = default);
}
