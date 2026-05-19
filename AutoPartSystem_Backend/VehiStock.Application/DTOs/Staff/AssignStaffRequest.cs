using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Staff;

public class AssignStaffRequest
{
    [Required]
    public int StaffMemberId { get; set; }
}
