using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class ServiceRecordService : IServiceRecordService
{
    private readonly IServiceRecordRepository _serviceRecordRepository;

    public ServiceRecordService(IServiceRecordRepository serviceRecordRepository)
    {
        _serviceRecordRepository = serviceRecordRepository;
    }

    public async Task<ServiceRecordResponse> CreateAsync(int customerId, int vehicleId, int staffMemberId, int? appointmentId, CancellationToken cancellationToken = default)
    {
        var serviceRecord = new ServiceRecord
        {
            CustomerId = customerId,
            VehicleId = vehicleId,
            StaffMemberId = staffMemberId,
            AppointmentId = appointmentId,
            ServiceDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Status = ServiceRecordStatus.Open,
            Diagnosis = string.Empty,
            WorkDone = string.Empty,
            LaborCharge = 0m,
            PartsCharge = 0m
        };

        var created = await _serviceRecordRepository.CreateAsync(serviceRecord, cancellationToken);
        return MapToResponse(created);
    }

    public async Task<ServiceRecordResponse> UpdateAsync(int serviceRecordId, UpdateServiceRecordRequest request, CancellationToken cancellationToken = default)
    {
        var record = await _serviceRecordRepository.GetByIdAsync(serviceRecordId, cancellationToken);
        if (record is null)
        {
            throw new InvalidOperationException("Service record not found.");
        }

        record.Diagnosis = request.Diagnosis.Trim();
        record.WorkDone = request.WorkDone.Trim();
        record.LaborCharge = request.LaborCharge;
        record.PartsCharge = request.PartsCharge;
        record.Notes = request.Notes?.Trim();

        // Auto-transition to ReadyForBilling if all required fields are filled
        if (record.Status == ServiceRecordStatus.Open && IsReadyForBilling(record))
        {
            record.Status = ServiceRecordStatus.ReadyForBilling;
        }

        var updated = await _serviceRecordRepository.UpdateAsync(record, cancellationToken);
        return MapToResponse(updated);
    }

    public async Task<ServiceRecordResponse> GetAsync(int serviceRecordId, CancellationToken cancellationToken = default)
    {
        var record = await _serviceRecordRepository.GetByIdAsync(serviceRecordId, cancellationToken);
        if (record is null)
        {
            throw new InvalidOperationException("Service record not found.");
        }

        return MapToResponse(record);
    }

    public async Task<ServiceRecordResponse> CreateFromAppointmentAsync(string staffUserId, CreateServiceRecordFromAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var staff = await _serviceRecordRepository.GetStaffProfileByUserIdAsync(staffUserId, cancellationToken);
        if (staff == null)
            throw new InvalidOperationException("Staff profile not found.");

        var appointment = await _serviceRecordRepository.GetAppointmentByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment == null)
            throw new InvalidOperationException("Appointment not found.");
        
        if (appointment.Status != AppointmentStatus.Confirmed)
            throw new InvalidOperationException("Appointment must be confirmed before creating a service record.");

        var serviceRecord = new ServiceRecord
        {
            CustomerId = appointment.CustomerId,
            VehicleId = appointment.VehicleId,
            StaffMemberId = staff.StaffMemberId,
            AppointmentId = appointment.AppointmentId,
            ServiceDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Status = ServiceRecordStatus.ReadyForBilling, // Ready for invoicing right away!
            Diagnosis = request.Diagnosis.Trim(),
            WorkDone = request.WorkDone.Trim(),
            LaborCharge = request.LaborCharge,
            PartsCharge = request.PartsCharge,
            Notes = request.Notes?.Trim()
        };

        var created = await _serviceRecordRepository.CreateAsync(serviceRecord, cancellationToken);
        
        // Let's assume updating the appointment is handled separately, 
        // or we can add it to the repo if needed. For now, the endpoint will handle it if needed.
        
        return MapToResponse(created);
    }

    public async Task<List<ServiceRecordResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await _serviceRecordRepository.GetAllAsync(cancellationToken);
        return records.Select(MapToResponse).ToList();
    }

    private static bool IsReadyForBilling(ServiceRecord record)
    {
        return !string.IsNullOrWhiteSpace(record.Diagnosis) &&
               !string.IsNullOrWhiteSpace(record.WorkDone) &&
               (record.LaborCharge > 0m || record.PartsCharge > 0m);
    }

    private static ServiceRecordResponse MapToResponse(ServiceRecord record)
    {
        return new ServiceRecordResponse
        {
            ServiceRecordId = record.ServiceRecordId,
            CustomerId = record.CustomerId,
            CustomerName = record.Customer != null && record.Customer.User != null ? record.Customer.User.FullName : string.Empty,
            VehicleId = record.VehicleId,
            VehicleNumber = record.Vehicle != null ? record.Vehicle.VehicleNumber : string.Empty,
            StaffMemberId = record.StaffMemberId,
            StaffName = record.StaffMember != null && record.StaffMember.User != null ? record.StaffMember.User.FullName : string.Empty,
            AppointmentId = record.AppointmentId,
            ServiceDate = record.ServiceDate,
            Status = record.Status.ToString(),
            Diagnosis = record.Diagnosis,
            WorkDone = record.WorkDone,
            LaborCharge = record.LaborCharge,
            PartsCharge = record.PartsCharge,
            TotalCharge = record.TotalCharge,
            Notes = record.Notes,
            ServiceInvoiceId = record.ServiceInvoice != null ? record.ServiceInvoice.ServiceInvoiceId : null,
            PartsUsed = record.PartsUsed != null
                ? record.PartsUsed.Select(pu => new ServiceRecordPartDto
                  {
                      ServiceRecordPartId = pu.ServiceRecordPartId,
                      PartId = pu.PartId,
                      PartName = pu.Part != null ? pu.Part.PartName : string.Empty,
                      Brand = pu.Part != null ? pu.Part.Brand : string.Empty,
                      Quantity = pu.Quantity,
                      UnitPrice = pu.UnitPrice,
                      LineTotal = pu.LineTotal
                  }).ToList()
                : new List<ServiceRecordPartDto>()
        };
    }
}
