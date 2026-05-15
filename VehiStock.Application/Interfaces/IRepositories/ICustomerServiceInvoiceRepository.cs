using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface ICustomerServiceInvoiceRepository
{
    Task<PaginatedResponse<ServiceInvoice>> GetServiceInvoicesPageAsync(int customerId, ServiceInvoiceQueryRequest request, CancellationToken cancellationToken = default);
    Task<ServiceInvoice?> GetServiceInvoiceForCustomerAsync(int customerId, int serviceInvoiceId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
