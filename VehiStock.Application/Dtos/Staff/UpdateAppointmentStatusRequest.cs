using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Staff;

public class UpdateAppointmentStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
