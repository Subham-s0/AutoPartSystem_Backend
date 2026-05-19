using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    public async Task<ServiceRecordResponse> CreateAsync(string staffUserId, CreateServiceRecordRequest request, CancellationToken cancellationToken = default)
    {
        var staff = await _serviceRecordRepository.GetStaffProfileByUserIdAsync(staffUserId, cancellationToken);
        if (staff == null)
            throw new InvalidOperationException("Staff profile not found.");

        var customer = await _serviceRecordRepository.GetCustomerAsync(request.CustomerId, cancellationToken);
        if (customer == null)
            throw new InvalidOperationException("Customer not found.");

        var vehicle = await _serviceRecordRepository.GetVehicleForCustomerAsync(request.CustomerId, request.VehicleId, cancellationToken);
        if (vehicle == null)
            throw new InvalidOperationException("Vehicle not found for this customer.");

        var partIds = request.PartsUsed.Select(x => x.PartId).Distinct().ToList();
        var parts = (await _serviceRecordRepository.GetPartsByIdsAsync(partIds, cancellationToken)).ToDictionary(x => x.PartId);
        if (parts.Count != partIds.Count)
            throw new InvalidOperationException("One or more parts were not found.");

        var serviceRecord = new ServiceRecord
        {
            CustomerId = request.CustomerId,
            VehicleId = request.VehicleId,
            StaffMemberId = staff.StaffMemberId,
            ServiceDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Status = ServiceRecordStatus.Open,
            Diagnosis = request.Diagnosis.Trim(),
            WorkDone = request.WorkDone.Trim(),
            LaborCharge = request.LaborCharge,
            Notes = request.Notes?.Trim()
        };

        decimal partsCharge = 0m;
        foreach (var item in request.PartsUsed)
        {
            var part = parts[item.PartId];
            part.DecreaseStock(item.Quantity);

            var lineTotal = part.UnitPrice * item.Quantity;
            partsCharge += lineTotal;

            serviceRecord.PartsUsed.Add(new ServiceRecordPart
            {
                PartId = part.PartId,
                Quantity = item.Quantity,
                UnitPrice = part.UnitPrice,
                LineTotal = lineTotal
            });
        }

        serviceRecord.PartsCharge = partsCharge;
        serviceRecord.TotalCharge = request.LaborCharge + partsCharge;

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ServiceRecordStatus>(request.Status, true, out var parsedStatus))
        {
            serviceRecord.Status = parsedStatus;
        }
        else if (IsReadyForBilling(serviceRecord))
        {
            serviceRecord.Status = ServiceRecordStatus.ReadyForBilling;
        }

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
        record.Notes = request.Notes?.Trim();

        // 1. Revert previous parts stock
        foreach (var oldPart in record.PartsUsed)
        {
            if (oldPart.Part != null)
            {
                oldPart.Part.IncreaseStock(oldPart.Quantity);
            }
        }

        // 2. Clear old parts
        record.PartsUsed.Clear();

        // 3. Fetch new parts
        var partIds = request.PartsUsed.Select(x => x.PartId).Distinct().ToList();
        var parts = (await _serviceRecordRepository.GetPartsByIdsAsync(partIds, cancellationToken)).ToDictionary(x => x.PartId);
        if (parts.Count != partIds.Count)
            throw new InvalidOperationException("One or more parts were not found.");

        decimal partsCharge = 0m;
        foreach (var item in request.PartsUsed)
        {
            var part = parts[item.PartId];
            part.DecreaseStock(item.Quantity);

            var lineTotal = part.UnitPrice * item.Quantity;
            partsCharge += lineTotal;

            record.PartsUsed.Add(new ServiceRecordPart
            {
                ServiceRecordId = serviceRecordId,
                PartId = part.PartId,
                Quantity = item.Quantity,
                UnitPrice = part.UnitPrice,
                LineTotal = lineTotal
            });
        }

        record.PartsCharge = partsCharge;
        record.TotalCharge = request.LaborCharge + partsCharge;

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ServiceRecordStatus>(request.Status, true, out var parsedStatus))
        {
            record.Status = parsedStatus;
        }
        else if (record.Status == ServiceRecordStatus.Open && IsReadyForBilling(record))
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
            Status = ServiceRecordStatus.ReadyForBilling,
            Diagnosis = request.Diagnosis.Trim(),
            WorkDone = request.WorkDone.Trim(),
            LaborCharge = request.LaborCharge,
            PartsCharge = request.PartsCharge,
            Notes = request.Notes?.Trim()
        };

        var created = await _serviceRecordRepository.CreateAsync(serviceRecord, cancellationToken);
        return MapToResponse(created);
    }

    public async Task<IReadOnlyCollection<ServiceRecordResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _serviceRecordRepository.GetListAsync(cancellationToken);
        return list.Select(MapToResponse).ToList();
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
            CustomerName = record.Customer?.User?.FullName ?? "Unknown Customer",
            VehicleId = record.VehicleId,
            VehicleNumber = record.Vehicle?.VehicleNumber ?? "Unknown Vehicle",
            StaffMemberId = record.StaffMemberId,
            StaffName = record.StaffMember?.User?.FullName ?? "Unknown Staff",
            AppointmentId = record.AppointmentId,
            ServiceDate = record.ServiceDate,
            Status = record.Status.ToString(),
            Diagnosis = record.Diagnosis,
            WorkDone = record.WorkDone,
            LaborCharge = record.LaborCharge,
            PartsCharge = record.PartsCharge,
            TotalCharge = record.TotalCharge,
            Notes = record.Notes,
            ServiceInvoiceId = record.ServiceInvoice?.ServiceInvoiceId,
            PartsUsed = record.PartsUsed.Select(x => new ServiceRecordPartResponse
            {
                ServiceRecordPartId = x.ServiceRecordPartId,
                PartId = x.PartId,
                PartName = x.Part?.PartName ?? "Unknown Part",
                Brand = x.Part?.Brand ?? string.Empty,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                LineTotal = x.LineTotal
            }).ToList()
        };
    }
}
