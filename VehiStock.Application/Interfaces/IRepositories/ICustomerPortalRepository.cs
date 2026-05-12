using VehiStock.Entities;
using VehiStock.Application.Dtos.Common;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface ICustomerPortalRepository
{
    Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetVehicleForCustomerAsync(int customerId, int vehicleId, CancellationToken cancellationToken = default);
    Task<PartRequest> CreatePartRequestAsync(PartRequest partRequest, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PartRequest>> GetPartRequestsByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<ServiceRecord?> GetServiceRecordForCustomerAsync(int customerId, int serviceRecordId, CancellationToken cancellationToken = default);
    Task<bool> HasReviewForServiceRecordAsync(int customerId, int serviceRecordId, CancellationToken cancellationToken = default);
    Task<Review> CreateReviewAsync(Review review, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<SalesInvoice>> GetPurchaseHistoryPageAsync(int customerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<ServiceRecord>> GetServiceHistoryPageAsync(int customerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SalesInvoice>> GetPurchaseHistoryAsync(int customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ServiceRecord>> GetServiceHistoryAsync(int customerId, CancellationToken cancellationToken = default);
}
