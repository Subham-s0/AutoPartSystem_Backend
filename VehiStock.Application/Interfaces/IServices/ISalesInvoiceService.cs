using VehiStock.Application.DTOs.SalesInvoices;

namespace VehiStock.Application.Interfaces.IServices;

public interface ISalesInvoiceService
{
    Task<SalesInvoiceDto> CreateSalesInvoiceAsync(CreateSalesInvoiceRequest request, CancellationToken cancellationToken = default);
}
