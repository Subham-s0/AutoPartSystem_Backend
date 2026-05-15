namespace VehiStock.Application.Dtos.Customer;

public class ServiceInvoiceListResponse
{
    public int ServiceInvoiceId { get; init; }

    public int ServiceRecordId { get; init; }

    public DateOnly ServiceDate { get; init; }

    public string VehicleNumber { get; init; } = string.Empty;

    public string Diagnosis { get; init; } = string.Empty;

    public string ServiceStatus { get; init; } = string.Empty;

    public string StaffMemberName { get; init; } = string.Empty;

    public decimal LaborCharge { get; init; }

    public decimal PartsCharge { get; init; }

    public decimal DiscountPercent { get; init; }

    public decimal TaxAmount { get; init; }

    public decimal TotalAmount { get; init; }

    public decimal AmountPaid { get; init; }

    public decimal BalanceDue { get; init; }

    public string PaymentStatus { get; init; } = string.Empty;
}
