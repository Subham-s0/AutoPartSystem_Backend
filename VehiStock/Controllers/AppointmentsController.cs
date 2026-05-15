using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Customer)]
[Route("api/customer/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> BookAppointment(
        BookAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _appointmentService.BookAppointmentAsync(GetCurrentUserId(), request, cancellationToken);
            return Ok(ApiResponse<AppointmentResponse>.Ok(appointment, "Appointment booked successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AppointmentResponse>.Fail(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<AppointmentResponse>>>> GetAppointments(
        [FromQuery] AppointmentQueryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var appointments = await _appointmentService.GetAppointmentsPageAsync(
                GetCurrentUserId(),
                request,
                cancellationToken);

            return Ok(ApiResponse<PaginatedResponse<AppointmentResponse>>.Ok(appointments, "Appointments fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PaginatedResponse<AppointmentResponse>>.Fail(ex.Message));
        }
    }

    [HttpGet("{appointmentId:int}")]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> GetAppointment(
        int appointmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _appointmentService.GetAppointmentAsync(
                GetCurrentUserId(),
                appointmentId,
                cancellationToken);

            return Ok(ApiResponse<AppointmentResponse>.Ok(appointment, "Appointment fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<AppointmentResponse>.Fail(ex.Message));
        }
    }

    [HttpPatch("{appointmentId:int}/cancel")]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> CancelAppointment(
        int appointmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _appointmentService.CancelAppointmentAsync(
                GetCurrentUserId(),
                appointmentId,
                cancellationToken);

            return Ok(ApiResponse<AppointmentResponse>.Ok(appointment, "Appointment cancelled successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AppointmentResponse>.Fail(ex.Message));
        }
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("Authenticated user ID is missing.");
        }

        return userId;
    }
}
