using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;

namespace VehiStock.Application.Interfaces.IServices;

public interface IAppointmentService
{
    Task<AppointmentResponse> BookAppointmentAsync(string userId, BookAppointmentRequest request, CancellationToken cancellationToken = default);

    Task<PaginatedResponse<AppointmentResponse>> GetAppointmentsPageAsync(
        string userId,
        AppointmentQueryRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentResponse> GetAppointmentAsync(
        string userId,
        int appointmentId,
        CancellationToken cancellationToken = default);

    Task<AppointmentResponse> CancelAppointmentAsync(
        string userId,
        int appointmentId,
        CancellationToken cancellationToken = default);
}
