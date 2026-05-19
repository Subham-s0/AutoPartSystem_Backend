using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Auth;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
