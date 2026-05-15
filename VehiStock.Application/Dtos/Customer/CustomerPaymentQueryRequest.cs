using VehiStock.Application.Dtos.Common;

namespace VehiStock.Application.Dtos.Customer;

public class CustomerPaymentQueryRequest
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? SearchText { get; set; }

    /// <summary>Service, Sales, or empty for all.</summary>
    public string? InvoiceKind { get; set; }

    public string? PaymentType { get; set; }

    /// <summary>Filters by the linked invoice payment status.</summary>
    public string? InvoiceStatus { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    public List<SortRequest> Sorts { get; set; } = [];
}
