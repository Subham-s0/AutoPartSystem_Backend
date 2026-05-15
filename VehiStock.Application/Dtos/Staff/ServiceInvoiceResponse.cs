namespace VehiStock.Application.Dtos.Staff;

public class ServiceInvoiceResponse
{
    public int ServiceInvoiceId { get; init; }
    public int ServiceRecordId { get; init; }
    public int CustomerId { get; init; }
    public int VehicleId { get; init; }
    public decimal LaborCharge { get; init; }
    public decimal PartsCharge { get; init; }
    public decimal DiscountPercent { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal AmountPaid { get; init; }
    public decimal BalanceDue { get; init; }
    public string PaymentStatus { get; init; } = string.Empty;
}
