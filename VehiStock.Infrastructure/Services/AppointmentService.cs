using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentService(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<AppointmentResponse> BookAppointmentAsync(string userId, BookAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        if (request.PreferredDate < DateOnly.FromDateTime(DateTime.UtcNow.Date))
        {
            throw new InvalidOperationException("Preferred date cannot be in the past.");
        }

        var vehicle = await _appointmentRepository.GetVehicleForCustomerAsync(customer.CustomerId, request.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            throw new InvalidOperationException("Vehicle not found for this customer.");
        }

        var appointment = new Appointment
        {
            CustomerId = customer.CustomerId,
            VehicleId = vehicle.VehicleId,
            PreferredDate = request.PreferredDate,
            ServiceType = request.ServiceType.Trim(),
            ProblemDescription = request.ProblemDescription.Trim(),
            Status = AppointmentStatus.Pending
        };

        var createdAppointment = await _appointmentRepository.CreateAppointmentAsync(appointment, cancellationToken);
        return MapAppointment(createdAppointment);
    }

    public async Task<PaginatedResponse<AppointmentResponse>> GetAppointmentsPageAsync(
        string userId,
        AppointmentQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var normalizedRequest = NormalizeRequest(request);
        var appointments = await _appointmentRepository.GetAppointmentsPageAsync(
            customer.CustomerId,
            normalizedRequest,
            cancellationToken);

        return new PaginatedResponse<AppointmentResponse>
        {
            Items = appointments.Items.Select(MapAppointment).ToList(),
            PageNumber = appointments.PageNumber,
            PageSize = appointments.PageSize,
            TotalRecords = appointments.TotalRecords,
            TotalPages = appointments.TotalPages
        };
    }

    public async Task<AppointmentResponse> CancelAppointmentAsync(
        string userId,
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var appointment = await _appointmentRepository.GetAppointmentForCustomerAsync(
            customer.CustomerId,
            appointmentId,
            cancellationToken);

        if (appointment is null)
        {
            throw new InvalidOperationException("Appointment not found for this customer.");
        }

        if (appointment.Status != AppointmentStatus.Pending)
        {
            throw new InvalidOperationException("Only pending appointments can be cancelled.");
        }

        appointment.Status = AppointmentStatus.Cancelled;
        var updatedAppointment = await _appointmentRepository.UpdateAppointmentAsync(appointment, cancellationToken);
        return MapAppointment(updatedAppointment);
    }

    private async Task<CustomerProfile> GetCustomerProfileAsync(string userId, CancellationToken cancellationToken)
    {
        var customer = await _appointmentRepository.GetCustomerProfileByUserIdAsync(userId, cancellationToken);
        if (customer is null)
        {
            throw new InvalidOperationException("Customer profile was not found for this account.");
        }

        return customer;
    }

    private static AppointmentResponse MapAppointment(Appointment appointment)
    {
        return new AppointmentResponse
        {
            AppointmentId = appointment.AppointmentId,
            VehicleId = appointment.VehicleId,
            VehicleNumber = appointment.Vehicle.VehicleNumber,
            PreferredDate = appointment.PreferredDate,
            ServiceType = appointment.ServiceType,
            ProblemDescription = appointment.ProblemDescription,
            Status = appointment.Status.ToString(),
            BookedAt = appointment.BookedAt
        };
    }

    private static AppointmentQueryRequest NormalizeRequest(AppointmentQueryRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            !Enum.TryParse<AppointmentStatus>(request.Status.Trim(), true, out _))
        {
            throw new InvalidOperationException("Invalid appointment status.");
        }

        return new AppointmentQueryRequest
        {
            PageNumber = Math.Max(1, request.PageNumber),
            PageSize = Math.Clamp(request.PageSize, 1, 100),
            SearchText = string.IsNullOrWhiteSpace(request.SearchText) ? null : request.SearchText.Trim(),
            Status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim(),
            SortBy = string.IsNullOrWhiteSpace(request.SortBy) ? "newest" : request.SortBy.Trim()
        };
    }
}
