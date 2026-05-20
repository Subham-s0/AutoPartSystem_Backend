using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;
using VehiStock.Entities;
using System.Security.Claims;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Staff)]
[Route("api/staff/service-records")]
public class StaffServiceRecordsController : ControllerBase
{
    private readonly IServiceRecordService _serviceRecordService;
    private readonly IStaffAppointmentService _staffAppointmentService;

    public StaffServiceRecordsController(
        IServiceRecordService serviceRecordService, 
        IStaffAppointmentService staffAppointmentService)
    {
        _serviceRecordService = serviceRecordService;
        _staffAppointmentService = staffAppointmentService;
    }

    [HttpPost("from-appointment")]
    public async Task<ActionResult<ApiResponse<ServiceRecordResponse>>> CreateFromAppointment(
        [FromBody] CreateServiceRecordFromAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(ApiResponse<ServiceRecordResponse>.Fail("User not authenticated"));

            var response = await _serviceRecordService.CreateFromAppointmentAsync(userId, request, cancellationToken);
            
            // Auto-update appointment status to Completed
            await _staffAppointmentService.UpdateStatusAsync(request.AppointmentId, AppointmentStatus.Completed.ToString(), cancellationToken);
            
            return Ok(ApiResponse<ServiceRecordResponse>.Ok(response, "Service record created and appointment marked as completed."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ServiceRecordResponse>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ServiceRecordResponse>.Fail("An unexpected error occurred: " + ex.Message));
        }
    }
}
