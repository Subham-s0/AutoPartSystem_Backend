using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Admin;

public class UpdatePartRequestStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
