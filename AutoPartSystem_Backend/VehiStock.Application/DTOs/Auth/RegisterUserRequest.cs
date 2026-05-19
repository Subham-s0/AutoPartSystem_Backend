using System.ComponentModel.DataAnnotations;
using VehiStock.Entities;

namespace VehiStock.Application.Dtos.Auth;

public class RegisterUserRequest
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

    [Required]
    public string Role { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    [MaxLength(500)]
    public string? ProfilePhotoUrl { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    public RegistrationSource? RegistrationSource { get; set; }

    [MaxLength(100)]
    public string? JobTitle { get; set; }

    public DateOnly? HireDate { get; set; }
}
