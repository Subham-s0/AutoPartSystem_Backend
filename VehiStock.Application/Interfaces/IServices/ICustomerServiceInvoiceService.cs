using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;

namespace VehiStock.Application.Interfaces.IServices;

public interface ICustomerServiceInvoiceService
{
    Task<PaginatedResponse<ServiceInvoiceListResponse>> GetServiceInvoicesPageAsync(string userId, ServiceInvoiceQueryRequest request, CancellationToken cancellationToken = default);
    Task<ServiceInvoiceListResponse> GetServiceInvoiceDetailAsync(string userId, int serviceInvoiceId, CancellationToken cancellationToken = default);
}
