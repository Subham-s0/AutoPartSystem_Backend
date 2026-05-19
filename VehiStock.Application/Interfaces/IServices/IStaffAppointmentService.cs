using System.Threading;
using System.Threading.Tasks;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Staff;

namespace VehiStock.Application.Interfaces.IServices;

public interface IStaffAppointmentService
{
    Task<PaginatedResponse<StaffAppointmentResponse>> GetAppointmentsPageAsync(
        int pageNumber,
        int pageSize,
        string? status,
        string? searchText,
        CancellationToken cancellationToken = default);

    Task<StaffAppointmentResponse> AcceptAppointmentAsync(
        int appointmentId,
        string staffUserId,
        CancellationToken cancellationToken = default);

    Task<StaffAppointmentResponse> UpdateStatusAsync(
        int appointmentId,
        string status,
        CancellationToken cancellationToken = default);

    Task<StaffAppointmentResponse> AssignStaffAsync(
        int appointmentId,
        int staffMemberId,
        CancellationToken cancellationToken = default);
}
