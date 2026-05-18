using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Staff)]
[Route("api/staff/appointments")]
public class StaffAppointmentsController : ControllerBase
{
    private readonly IStaffAppointmentService _appointmentService;

    public StaffAppointmentsController(IStaffAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<StaffAppointmentResponse>>>> GetAppointments(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? searchText = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _appointmentService.GetAppointmentsPageAsync(
            pageNumber,
            pageSize,
            status,
            searchText,
            cancellationToken);

        return Ok(ApiResponse<PaginatedResponse<StaffAppointmentResponse>>.Ok(response, "Appointments fetched successfully."));
    }

    [HttpPatch("{appointmentId:int}/status")]
    public async Task<ActionResult<ApiResponse<StaffAppointmentResponse>>> UpdateStatus(
        int appointmentId,
        [FromBody] UpdateAppointmentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _appointmentService.UpdateStatusAsync(appointmentId, request.Status, cancellationToken);
            return Ok(ApiResponse<StaffAppointmentResponse>.Ok(response, "Appointment status updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<StaffAppointmentResponse>.Fail(ex.Message));
        }
    }

    [HttpPatch("{appointmentId:int}/assign")]
    public async Task<ActionResult<ApiResponse<StaffAppointmentResponse>>> AssignStaff(
        int appointmentId,
        [FromBody] AssignStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _appointmentService.AssignStaffAsync(appointmentId, request.StaffMemberId, cancellationToken);
            return Ok(ApiResponse<StaffAppointmentResponse>.Ok(response, "Staff member assigned successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<StaffAppointmentResponse>.Fail(ex.Message));
        }
    }
}
