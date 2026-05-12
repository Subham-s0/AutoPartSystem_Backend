using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface IAppointmentRepository
{
    Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<Vehicle?> GetVehicleForCustomerAsync(int customerId, int vehicleId, CancellationToken cancellationToken = default);

    Task<Appointment> CreateAppointmentAsync(Appointment appointment, CancellationToken cancellationToken = default);

    Task<Appointment?> GetAppointmentForCustomerAsync(
        int customerId,
        int appointmentId,
        CancellationToken cancellationToken = default);

    Task<Appointment> UpdateAppointmentAsync(Appointment appointment, CancellationToken cancellationToken = default);

    Task<PaginatedResponse<Appointment>> GetAppointmentsPageAsync(
        int customerId,
        AppointmentQueryRequest request,
        CancellationToken cancellationToken = default);
}
