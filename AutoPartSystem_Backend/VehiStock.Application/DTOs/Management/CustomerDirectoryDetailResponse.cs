using VehiStock.Entities;

namespace VehiStock.Application.Dtos.Management;

public class CustomerDirectoryDetailResponse
{
    public int CustomerId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public bool IsActive { get; set; }
    public string Address { get; set; } = string.Empty;
    public RegistrationSource RegistrationSource { get; set; }
    public DateTime RegisteredAt { get; set; }
    public IReadOnlyCollection<CustomerDirectoryVehicleResponse> Vehicles { get; set; } = [];
    public IReadOnlyCollection<CustomerDirectoryInvoiceResponse> SalesInvoices { get; set; } = [];
    public IReadOnlyCollection<CustomerDirectoryPaymentResponse> Payments { get; set; } = [];
    public CustomerReportSnapshotResponse ReportSnapshot { get; set; } = new();
}
