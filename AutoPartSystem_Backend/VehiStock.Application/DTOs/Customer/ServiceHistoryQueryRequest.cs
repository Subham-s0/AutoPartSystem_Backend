using VehiStock.Application.Dtos.Common;

namespace VehiStock.Application.Dtos.Customer;

public class ServiceHistoryQueryRequest
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? SearchText { get; set; }

    public string? Status { get; set; }

    /// <summary>
    /// Filter by invoice status. Accepts a <see cref="VehiStock.Entities.PaymentStatus"/> value
    /// or the special value "NotInvoiced" to return records that have no invoice yet.
    /// </summary>
    public string? InvoiceStatus { get; set; }

    public List<SortRequest> Sorts { get; set; } = [];
}
