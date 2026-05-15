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
            VehicleId = record.VehicleId,
            StaffMemberId = record.StaffMemberId,
            AppointmentId = record.AppointmentId,
            ServiceDate = record.ServiceDate,
            Status = record.Status.ToString(),
            Diagnosis = record.Diagnosis,
            WorkDone = record.WorkDone,
            LaborCharge = record.LaborCharge,
            PartsCharge = record.PartsCharge,
            TotalCharge = record.TotalCharge,
            Notes = record.Notes
        };
    }
}
