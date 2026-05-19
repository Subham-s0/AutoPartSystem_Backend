using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Domain.Constants;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.Staff},{RoleNames.Admin}")]
[Route("api/staff/appointments")]
public class StaffAppointmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StaffAppointmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<StaffAppointmentResponse>>>> GetAppointments(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? searchText = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Appointments
                .Include(a => a.Customer).ThenInclude(c => c.User)
                .Include(a => a.Vehicle)
                .Include(a => a.AssignedStaff).ThenInclude(s => s.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<AppointmentStatus>(status, true, out var parsedStatus))
                {
                    query = query.Where(a => a.Status == parsedStatus);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var s = searchText.Trim().ToLower();
                query = query.Where(a =>
                    a.Customer.User.FullName.ToLower().Contains(s) ||
                    (a.Customer.User.Email != null && a.Customer.User.Email.ToLower().Contains(s)) ||
                    a.Vehicle.VehicleNumber.ToLower().Contains(s) ||
                    a.ServiceType.ToLower().Contains(s) ||
                    a.ProblemDescription.ToLower().Contains(s)
                );
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(a => a.BookedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new StaffAppointmentResponse
                {
                    AppointmentId = a.AppointmentId,
                    CustomerId = a.CustomerId,
                    CustomerName = a.Customer.User.FullName,
                    CustomerEmail = a.Customer.User.Email ?? string.Empty,
                    VehicleId = a.VehicleId,
                    VehicleNumber = a.Vehicle.VehicleNumber,
                    PreferredDate = a.PreferredDate.ToString("yyyy-MM-dd"),
                    ServiceType = a.ServiceType,
                    ProblemDescription = a.ProblemDescription,
                    Status = a.Status.ToString(),
                    AssignedStaffId = a.AssignedStaffId,
                    AssignedStaffName = a.AssignedStaff != null ? a.AssignedStaff.User.FullName : null,
                    BookedAt = a.BookedAt
                })
                .ToListAsync(cancellationToken);

            var response = new PaginatedResponse<StaffAppointmentResponse>
            {
                Items = items,
                TotalRecords = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(ApiResponse<PaginatedResponse<StaffAppointmentResponse>>.Ok(response, "Appointments fetched successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PaginatedResponse<StaffAppointmentResponse>>.Fail("An error occurred: " + ex.Message));
        }
    }

    [HttpPut("{appointmentId:int}/status")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStatus(
        int appointmentId,
        [FromBody] UpdateStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, cancellationToken);

            if (appointment == null)
            {
                return NotFound(ApiResponse<object>.Fail("Appointment not found."));
            }

            if (!Enum.TryParse<AppointmentStatus>(request.Status, true, out var parsedStatus))
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid status value."));
            }

            appointment.Status = parsedStatus;
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null!, "Appointment status updated successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail("An error occurred: " + ex.Message));
        }
    }

    [HttpPost("{appointmentId:int}/assign")]
    public async Task<ActionResult<ApiResponse<object>>> AssignStaff(
        int appointmentId,
        [FromBody] AssignStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, cancellationToken);

            if (appointment == null)
            {
                return NotFound(ApiResponse<object>.Fail("Appointment not found."));
            }

            var staffExists = await _context.StaffProfiles.AnyAsync(s => s.StaffMemberId == request.StaffId, cancellationToken);
            if (!staffExists)
            {
                return BadRequest(ApiResponse<object>.Fail("Staff member not found."));
            }

            appointment.AssignedStaffId = request.StaffId;
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null!, "Staff assigned successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail("An error occurred: " + ex.Message));
        }
    }
}

public class StaffAppointmentResponse
{
    public int AppointmentId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string PreferredDate { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? AssignedStaffId { get; set; }
    public string? AssignedStaffName { get; set; }
    public DateTime BookedAt { get; set; }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class AssignStaffRequest
{
    public int StaffId { get; set; }
}
