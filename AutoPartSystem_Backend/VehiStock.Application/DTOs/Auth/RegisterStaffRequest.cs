using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Auth;

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

    [MaxLength(500)]
    public string? ProfilePhotoUrl { get; set; }

    [Required]
    [MaxLength(100)]
    public string JobTitle { get; set; } = string.Empty;

    public DateOnly? HireDate { get; set; }
}
