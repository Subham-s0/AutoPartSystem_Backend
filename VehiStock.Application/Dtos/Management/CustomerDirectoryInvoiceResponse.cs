using VehiStock.Entities;

namespace VehiStock.Application.Dtos.Management;

public class CustomerDirectoryInvoiceResponse
{
    public int SalesInvoiceId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
}
