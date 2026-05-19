using VehiStock.Application.Dtos.Customer;

namespace VehiStock.Application.Interfaces.IServices;

public interface ISalesInvoicePaymentService
{
    Task<InvoicePaymentInitiateResponse> InitiateAsync(
        string userId,
        int salesInvoiceId,
        InvoicePaymentInitiateRequest request,
        CancellationToken cancellationToken = default);

    Task<SalesInvoiceLoyaltyResponse> SetLoyaltyAsync(
        string userId,
        int salesInvoiceId,
        SetSalesInvoiceLoyaltyRequest request,
        CancellationToken cancellationToken = default);
}
