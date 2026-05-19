namespace VehiStock.Application.Dtos.Staff;

// Used for pending credit report response
public class PendingCreditReportResponse
{
    public int SalesInvoiceId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public DateOnly? CreditDueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }
}
