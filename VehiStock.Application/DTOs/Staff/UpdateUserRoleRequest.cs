using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.DTOs.Staff;

public class UpdateUserRoleRequest
{
    [Required]
    public string Role { get; set; } = string.Empty;
}
