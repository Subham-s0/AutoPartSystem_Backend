using VehiStock.Application.DTOs.SalesInvoices;

namespace VehiStock.Application.Interfaces.IServices;

// Used for sales invoice management
public interface ISalesInvoiceService
{
    Task<SalesInvoiceDto> CreateSalesInvoiceAsync(CreateSalesInvoiceRequest request, CancellationToken cancellationToken = default);
}
