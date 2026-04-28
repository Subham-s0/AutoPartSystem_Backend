using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

// Used for sales invoice data access
public interface ISalesInvoiceRepository
{
    Task<StaffProfile?> GetStaffProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> InvoiceExistsAsync(string invoiceNo, CancellationToken cancellationToken = default);
    Task<CustomerProfile?> GetCustomerAsync(int customerId, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetVehicleForCustomerAsync(int customerId, int vehicleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Part>> GetPartsByIdsAsync(IReadOnlyCollection<int> partIds, CancellationToken cancellationToken = default);
    Task<SalesInvoice> CreateSalesInvoiceAsync(SalesInvoice salesInvoice, Payment? payment, CancellationToken cancellationToken = default);
}
