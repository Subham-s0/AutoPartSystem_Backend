using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface ICustomerHistoryRepository
{
    Task<PaginatedResponse<SalesInvoice>> GetPurchaseHistoryPageAsync(int customerId, PurchaseHistoryQueryRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SalesInvoice>> GetPurchaseHistoryAsync(int customerId, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<ServiceRecord>> GetServiceHistoryPageAsync(int customerId, ServiceHistoryQueryRequest request, CancellationToken cancellationToken = default);
    Task<ServiceRecord?> GetServiceRecordDetailAsync(int customerId, int serviceRecordId, CancellationToken cancellationToken = default);
}
