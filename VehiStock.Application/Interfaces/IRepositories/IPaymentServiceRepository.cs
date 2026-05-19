using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface IPaymentServiceRepository
{
    Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<Payment>> GetPaymentsPageAsync(int customerId, CustomerPaymentQueryRequest request, CancellationToken cancellationToken = default);
    Task<bool> PaymentExistsForKhaltiTransactionAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<ServiceInvoice?> GetServiceInvoiceForCustomerAsync(int customerId, int serviceInvoiceId, CancellationToken cancellationToken = default);
    Task<SalesInvoice?> GetSalesInvoiceForCustomerAsync(int customerId, int salesInvoiceId, CancellationToken cancellationToken = default);
    Task AddPaymentAndSaveAsync(Payment payment, ServiceInvoice serviceInvoice, CancellationToken cancellationToken = default);
    Task AddSalesInvoicePaymentAndSaveAsync(Payment payment, SalesInvoice salesInvoice, CancellationToken cancellationToken = default);
    Task SaveSalesInvoiceAsync(SalesInvoice salesInvoice, CancellationToken cancellationToken = default);
}
