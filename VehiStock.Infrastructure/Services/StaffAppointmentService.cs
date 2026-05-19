using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Services;

public class StaffAppointmentService : IStaffAppointmentService
{
    private readonly ApplicationDbContext _dbContext;

    public StaffAppointmentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResponse<StaffAppointmentResponse>> GetAppointmentsPageAsync(
        int pageNumber,
        int pageSize,
        string? status,
        string? searchText,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Appointments
            .Include(x => x.Customer).ThenInclude(c => c.User)
            .Include(x => x.Vehicle)
            .Include(x => x.AssignedStaff).ThenInclude(s => s!.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.Trim().ToLower();
            query = query.Where(x =>
                x.Vehicle.VehicleNumber.ToLower().Contains(search) ||
                x.ServiceType.ToLower().Contains(search) ||
                x.Customer!.User!.FullName.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<AppointmentStatus>(status.Trim(), true, out var appStatus))
        {
            query = query.Where(x => x.Status == appStatus);
        }

        query = query.OrderByDescending(x => x.BookedAt).ThenByDescending(x => x.AppointmentId);

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new StaffAppointmentResponse
            {
                AppointmentId = a.AppointmentId,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer!.User!.FullName,
                CustomerEmail = a.Customer.User.Email ?? string.Empty,
                VehicleId = a.VehicleId,
                VehicleNumber = a.Vehicle.VehicleNumber,
                PreferredDate = a.PreferredDate,
                ServiceType = a.ServiceType,
                ProblemDescription = a.ProblemDescription,
                Status = a.Status.ToString(),
                AssignedStaffId = a.AssignedStaffId,
                AssignedStaffName = a.AssignedStaff != null ? a.AssignedStaff.User!.FullName : null,
                BookedAt = a.BookedAt
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<StaffAppointmentResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)pageSize)
        };
    }

    public async Task<StaffAppointmentResponse> AcceptAppointmentAsync(
        int appointmentId,
        string staffUserId,
        CancellationToken cancellationToken = default)
    {
        var appointment = await LoadAppointmentAsync(appointmentId, cancellationToken);
        if (appointment == null)
        {
            throw new InvalidOperationException("Appointment not found.");
        }

        if (appointment.Status != AppointmentStatus.Pending)
        {
            throw new InvalidOperationException("Only pending appointments can be accepted.");
        }

        var staff = await _dbContext.StaffProfiles
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.UserId == staffUserId, cancellationToken);

        if (staff == null)
        {
            throw new InvalidOperationException("Staff profile was not found for this account.");
        }

        appointment.Status = AppointmentStatus.Confirmed;
        appointment.AssignedStaffId = staff.StaffMemberId;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(appointment);
    }

    public async Task<StaffAppointmentResponse> UpdateStatusAsync(
        int appointmentId,
        string status,
        CancellationToken cancellationToken = default)
    {
        var appointment = await LoadAppointmentAsync(appointmentId, cancellationToken);
        if (appointment == null)
        {
            throw new InvalidOperationException("Appointment not found.");
        }

        if (!Enum.TryParse<AppointmentStatus>(status.Trim(), true, out var newStatus))
        {
            throw new InvalidOperationException($"Invalid appointment status. Allowed values are: {string.Join(", ", Enum.GetNames(typeof(AppointmentStatus)))}");
        }

        ValidateStatusTransition(appointment.Status, newStatus);

        appointment.Status = newStatus;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(appointment);
    }

    public async Task<StaffAppointmentResponse> AssignStaffAsync(
        int appointmentId,
        int staffMemberId,
        CancellationToken cancellationToken = default)
    {
        var appointment = await LoadAppointmentAsync(appointmentId, cancellationToken);
        if (appointment == null)
        {
            throw new InvalidOperationException("Appointment not found.");
        }

        if (appointment.Status is AppointmentStatus.Pending or AppointmentStatus.Cancelled or AppointmentStatus.Completed)
        {
            throw new InvalidOperationException("Staff can only be reassigned on confirmed appointments.");
        }

        var staffExists = await _dbContext.StaffProfiles
            .AnyAsync(x => x.StaffMemberId == staffMemberId, cancellationToken);

        if (!staffExists)
        {
            throw new InvalidOperationException("Staff member not found.");
        }

        appointment.AssignedStaffId = staffMemberId;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(appointment);
    }

    private Task<Appointment?> LoadAppointmentAsync(int appointmentId, CancellationToken cancellationToken)
    {
        return _dbContext.Appointments
            .Include(x => x.Customer).ThenInclude(c => c.User)
            .Include(x => x.Vehicle)
            .Include(x => x.AssignedStaff).ThenInclude(s => s!.User)
            .SingleOrDefaultAsync(x => x.AppointmentId == appointmentId, cancellationToken);
    }

    private static void ValidateStatusTransition(AppointmentStatus currentStatus, AppointmentStatus newStatus)
    {
        if (currentStatus == newStatus)
        {
            return;
        }

        var allowed = (currentStatus, newStatus) switch
        {
            (AppointmentStatus.Pending, AppointmentStatus.Cancelled) => true,
            (AppointmentStatus.Confirmed, AppointmentStatus.Completed) => true,
            _ => false
        };

        if (!allowed)
        {
            throw new InvalidOperationException(
                $"Cannot change appointment status from {currentStatus} to {newStatus}. Accept pending requests first, or complete confirmed jobs.");
        }
    }

    private static StaffAppointmentResponse MapToResponse(Appointment a)
    {
        return new StaffAppointmentResponse
        {
            AppointmentId = a.AppointmentId,
            CustomerId = a.CustomerId,
            CustomerName = a.Customer!.User!.FullName,
            CustomerEmail = a.Customer.User.Email ?? string.Empty,
            VehicleId = a.VehicleId,
            VehicleNumber = a.Vehicle.VehicleNumber,
            PreferredDate = a.PreferredDate,
            ServiceType = a.ServiceType,
            ProblemDescription = a.ProblemDescription,
            Status = a.Status.ToString(),
            AssignedStaffId = a.AssignedStaffId,
            AssignedStaffName = a.AssignedStaff != null ? a.AssignedStaff.User!.FullName : null,
            BookedAt = a.BookedAt
        };
    }
}
