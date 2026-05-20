using VehiStock.Application.Dtos.Staff;

namespace VehiStock.Application.Interfaces.IServices;

public interface IServiceInvoiceService
{
    Task<ServiceInvoiceResponse> CreateAsync(int serviceRecordId, CancellationToken cancellationToken = default);
}
