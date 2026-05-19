namespace VehiStock.Application.Dtos.Management;

public class StaffDetailResponse
{
    public string UserId { get; set; } = string.Empty;
    public int StaffMemberId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public bool IsActive { get; set; }
    public string Role { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
    public int TotalInvoicesCreated { get; set; }
    public decimal TotalInvoiceValue { get; set; }
    public IReadOnlyCollection<StaffInvoiceActivityResponse> RecentInvoices { get; set; } = [];
}
