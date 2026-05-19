using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Staff;

public class CreateServiceRecordRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Customer is required.")]
    public int CustomerId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Vehicle is required.")]
    public int VehicleId { get; set; }

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

public class CreateServiceRecordPartRequest
{
    [Required]
    public int PartId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}
