using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Staff;

public class UpdateServiceRecordRequest
{
    [Required]
    public string Diagnosis { get; set; } = string.Empty;

    [Required]
    public string WorkDone { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal LaborCharge { get; set; }

    public string? Notes { get; set; }

    public string? Status { get; set; }

    public List<CreateServiceRecordPartRequest> PartsUsed { get; set; } = [];
}
