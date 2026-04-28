using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
