using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;

namespace VehiStock.Application.Interfaces.IServices;

public interface ICustomerHistoryService
{
    Task<PaginatedResponse<PurchaseHistoryResponse>> GetPurchaseHistoryPageAsync(string userId, PurchaseHistoryQueryRequest request, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<ServiceHistoryResponse>> GetServiceHistoryPageAsync(string userId, ServiceHistoryQueryRequest request, CancellationToken cancellationToken = default);
    Task<ServiceHistoryResponse> GetServiceHistoryDetailAsync(string userId, int serviceRecordId, CancellationToken cancellationToken = default);
}
