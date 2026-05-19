using VehiStock.Entities;

namespace VehiStock.Application.Dtos.Management;

public class StaffInvoiceActivityResponse
{
    public int SalesInvoiceId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
}
