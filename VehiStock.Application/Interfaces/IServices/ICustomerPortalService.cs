using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Dtos.Common;

namespace VehiStock.Application.Interfaces.IServices;

public interface ICustomerPortalService
{
    Task<PartRequestResponse> CreatePartRequestAsync(string userId, CreatePartRequestRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PartRequestResponse>> GetPartRequestsAsync(string userId, CancellationToken cancellationToken = default);
    Task<ReviewResponse> CreateReviewAsync(string userId, CreateReviewRequest request, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<PurchaseHistoryResponse>> GetPurchaseHistoryPageAsync(string userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<ServiceHistoryResponse>> GetServiceHistoryPageAsync(string userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<CustomerHistoryResponse> GetHistoryAsync(string userId, CancellationToken cancellationToken = default);
}
