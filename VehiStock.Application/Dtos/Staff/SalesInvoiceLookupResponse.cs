namespace VehiStock.Application.Dtos.Staff;

public class SalesInvoiceLookupResponse
{
    public IReadOnlyCollection<SalesInvoiceCustomerLookupResponse> Customers { get; set; } = [];

    public IReadOnlyCollection<SalesInvoicePartLookupResponse> Parts { get; set; } = [];
}
