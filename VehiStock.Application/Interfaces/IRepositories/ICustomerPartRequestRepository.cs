using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface ICustomerPartRequestRepository
{
    Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<Vehicle?> GetVehicleForCustomerAsync(int customerId, int vehicleId, CancellationToken cancellationToken = default);

    Task<PartRequest> CreatePartRequestAsync(PartRequest partRequest, CancellationToken cancellationToken = default);

    Task<PaginatedResponse<PartRequest>> GetPartRequestsPageAsync(
        int customerId,
        PartRequestQueryRequest query,
        CancellationToken cancellationToken = default);

    Task<PartRequest?> GetPartRequestForCustomerAsync(
        int customerId,
        int partRequestId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
