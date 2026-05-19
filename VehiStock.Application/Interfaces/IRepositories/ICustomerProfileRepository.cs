using VehiStock.Application.Dtos.Management;
using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface ICustomerProfileRepository
{
    Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<CustomerDirectoryItemResponse> Items, int TotalRecords)> GetCustomersAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<CustomerProfile?> GetCustomerDetailByIdAsync(int customerId, CancellationToken cancellationToken = default);
}
