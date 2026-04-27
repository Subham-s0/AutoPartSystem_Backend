using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.DTOs.Staff;

public class RegisterStaffRequest
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    [Required]
    [MaxLength(50)]
    public string StaffCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string JobTitle { get; set; } = string.Empty;

    public DateOnly HireDate { get; set; }

    [Required]
    public string Role { get; set; } = string.Empty;
}
