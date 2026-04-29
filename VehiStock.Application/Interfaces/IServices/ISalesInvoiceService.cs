using VehiStock.Application.Dtos.Staff;

namespace VehiStock.Application.Interfaces.IServices;

// Used for sales invoice management
public interface ISalesInvoiceService
{
    Task<SalesInvoiceResponse> CreateAsync(string userId, CreateSalesInvoiceRequest request, CancellationToken cancellationToken = default);

    Task<SalesInvoiceLookupResponse> GetLookupAsync(CancellationToken cancellationToken = default);
}
