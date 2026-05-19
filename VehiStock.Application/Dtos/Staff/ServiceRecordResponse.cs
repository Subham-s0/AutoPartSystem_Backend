using System;
using System.Collections.Generic;

namespace VehiStock.Application.Dtos.Staff;

public class ServiceRecordResponse
{
    public int ServiceRecordId { get; init; }
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public int VehicleId { get; init; }
    public string VehicleNumber { get; init; } = string.Empty;
    public int StaffMemberId { get; init; }
    public string StaffName { get; init; } = string.Empty;
    public int? AppointmentId { get; init; }
    public DateOnly ServiceDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Diagnosis { get; init; } = string.Empty;
    public string WorkDone { get; init; } = string.Empty;
    public decimal LaborCharge { get; init; }
    public decimal PartsCharge { get; init; }
    public decimal TotalCharge { get; init; }
    public string? Notes { get; init; }
    public int? ServiceInvoiceId { get; init; }
    public List<ServiceRecordPartResponse> PartsUsed { get; init; } = [];
}
