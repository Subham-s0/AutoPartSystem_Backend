using VehiStock.Application.Dtos.Customer;

namespace VehiStock.Application.Interfaces.IServices;

public interface IServiceInvoicePaymentService
{
    Task<InvoicePaymentInitiateResponse> InitiateAsync(
        string userId,
        int serviceInvoiceId,
        InvoicePaymentInitiateRequest request,
        CancellationToken cancellationToken = default);

    Task<InvoicePaymentVerifyResponse> VerifyAsync(
        string userId,
        InvoicePaymentVerifyRequest request,
        CancellationToken cancellationToken = default);
}
