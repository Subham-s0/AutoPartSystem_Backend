using VehiStock.Entities;

namespace VehiStock.Application.Dtos.Management;

public class CustomerDirectoryPaymentResponse
{
    public int PaymentId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentType PaymentType { get; set; }
    public string? Notes { get; set; }
    public int? SalesInvoiceId { get; set; }
    public string? InvoiceNo { get; set; }
}
