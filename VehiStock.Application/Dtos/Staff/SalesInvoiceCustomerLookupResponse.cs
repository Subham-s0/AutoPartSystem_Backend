namespace VehiStock.Application.Dtos.Staff;

public class SalesInvoiceCustomerLookupResponse
{
    public int CustomerId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public IReadOnlyCollection<SalesInvoiceVehicleLookupResponse> Vehicles { get; set; } = [];
}
