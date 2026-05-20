namespace VehiStock.Application.Dtos.Staff;

public class ServiceInvoiceResponse
{
    public int ServiceInvoiceId { get; init; }
    public string InvoiceNo { get; init; } = string.Empty;
    public int ServiceRecordId { get; init; }
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public int VehicleId { get; init; }
    public string VehicleNumber { get; init; } = string.Empty;
    public decimal LaborCharge { get; init; }
    public decimal PartsCharge { get; init; }
    public decimal DiscountPercent { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal AmountPaid { get; init; }
    public decimal BalanceDue { get; init; }
    public string PaymentStatus { get; init; } = string.Empty;
    public string InvoiceDate { get; init; } = string.Empty;
}
