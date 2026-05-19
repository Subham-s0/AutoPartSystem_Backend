using System.ComponentModel.DataAnnotations;
using VehiStock.Entities;

namespace VehiStock.Application.Dtos.Auth;

public class CustomerSelfRegisterRequest
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
    [MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    public RegistrationSource? RegistrationSource { get; set; }
}
