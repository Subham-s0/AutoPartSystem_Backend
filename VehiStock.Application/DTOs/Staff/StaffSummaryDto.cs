namespace VehiStock.Application.DTOs.Staff;

public class StaffSummaryDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string StaffCode { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
