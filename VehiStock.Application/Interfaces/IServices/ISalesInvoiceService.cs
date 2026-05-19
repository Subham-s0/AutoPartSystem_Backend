using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Staff;

namespace VehiStock.Application.Interfaces.IServices;

// Used for sales invoice management
public interface ISalesInvoiceService
{
    Task<SalesInvoiceResponse> CreateAsync(string userId, CreateSalesInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<SalesInvoiceLookupResponse> GetLookupAsync(CancellationToken cancellationToken = default);
    Task<PaginatedResponse<SalesInvoiceResponse>> GetPaginatedAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<SalesInvoiceResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task SendEmailAsync(int id, CancellationToken cancellationToken = default);
}
