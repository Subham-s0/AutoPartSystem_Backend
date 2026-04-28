using VehiStock.Entities;

namespace VehiStock.Application.Dtos.Staff;

// Used for sales invoice response
public class SalesInvoiceResponse
{
    public int SalesInvoiceId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int VehicleId { get; set; }
    public int StaffMemberId { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }
    public DateOnly? CreditDueDate { get; set; }
    public PaymentType PaymentType { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public IReadOnlyCollection<SalesInvoiceItemResponse> Items { get; set; } = [];
}
