using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Admin;

// Used for staff role update request
public class UpdateStaffRoleRequest
{
    [Required]
    public string Role { get; set; } = string.Empty;
}
