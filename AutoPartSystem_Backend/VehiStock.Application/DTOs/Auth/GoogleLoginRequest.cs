using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Auth;

public class GoogleLoginRequest
{
    [Required]
    public string IdToken { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Address { get; set; }
}
