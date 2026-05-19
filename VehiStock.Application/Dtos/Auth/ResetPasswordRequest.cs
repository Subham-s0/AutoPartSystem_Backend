using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Auth;

public class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public string NewPassword { get; set; } = string.Empty;
}
