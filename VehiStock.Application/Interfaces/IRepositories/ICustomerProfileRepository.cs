using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Management;
using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface ICustomerProfileRepository
{
    Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<CustomerProfile?> GetCustomerProfileByIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<CustomerProfile>> GetCustomersForStaffAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<CustomerDirectoryItemResponse> Items, int TotalRecords)> GetCustomersAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<CustomerProfile?> GetCustomerDetailByIdAsync(int customerId, CancellationToken cancellationToken = default);
}
