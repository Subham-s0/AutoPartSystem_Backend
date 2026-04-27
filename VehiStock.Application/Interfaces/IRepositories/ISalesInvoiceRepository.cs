using VehiStock.Application.DTOs.SalesInvoices;
using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

// Used for sales invoice data access
public interface ISalesInvoiceRepository
{
    Task<bool> SalesInvoiceExistsAsync(string invoiceNo, CancellationToken cancellationToken = default);
    Task<CustomerProfile?> GetCustomerAsync(int customerId, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetVehicleForCustomerAsync(int vehicleId, int customerId, CancellationToken cancellationToken = default);
    Task<StaffProfile?> GetStaffMemberAsync(int staffMemberId, CancellationToken cancellationToken = default);
    Task<Dictionary<int, Part>> GetPartsByIdsAsync(IReadOnlyCollection<int> partIds, CancellationToken cancellationToken = default);
    Task<SalesInvoiceDto> CreateSalesInvoiceAsync(SalesInvoice salesInvoice, Payment? payment, IReadOnlyList<SalesInvoiceItemDto> responseItems, CancellationToken cancellationToken = default);
}
