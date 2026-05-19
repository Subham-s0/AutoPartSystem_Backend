namespace VehiStock.Application.Dtos.Admin;

// Used for staff list and role management response
public class StaffSummaryResponse
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int StaffMemberId { get; set; }
}
