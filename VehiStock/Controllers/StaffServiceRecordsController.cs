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
[Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Staff}")]
[Route("api/staff/service-records")]
public class StaffServiceRecordsController : ControllerBase
{
    private readonly IServiceRecordService _serviceRecordService;
    private readonly IStaffAppointmentService _staffAppointmentService;
    private readonly IServiceInvoiceService _serviceInvoiceService;
    private readonly ISalesInvoiceService _salesInvoiceService;

    public StaffServiceRecordsController(
        IServiceRecordService serviceRecordService, 
        IStaffAppointmentService staffAppointmentService,
        IServiceInvoiceService serviceInvoiceService,
        ISalesInvoiceService salesInvoiceService)
    {
        _serviceRecordService = serviceRecordService;
        _staffAppointmentService = staffAppointmentService;
        _serviceInvoiceService = serviceInvoiceService;
        _salesInvoiceService = salesInvoiceService;
    }

    [HttpGet("lookups")]
    public async Task<ActionResult<ApiResponse<SalesInvoiceLookupResponse>>> GetLookups(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _salesInvoiceService.GetLookupAsync(cancellationToken);
            return Ok(ApiResponse<SalesInvoiceLookupResponse>.Ok(response, "Service record lookups fetched successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SalesInvoiceLookupResponse>.Fail("An unexpected error occurred: " + ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ServiceRecordResponse>>>> GetServiceRecords(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var records = await _serviceRecordService.GetAllAsync(cancellationToken);
            return Ok(ApiResponse<List<ServiceRecordResponse>>.Ok(records, "Service records fetched successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<ServiceRecordResponse>>.Fail("An unexpected error occurred: " + ex.Message));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ServiceRecordResponse>>> GetById(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var record = await _serviceRecordService.GetAsync(id, cancellationToken);
            return Ok(ApiResponse<ServiceRecordResponse>.Ok(record, "Service record fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<ServiceRecordResponse>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ServiceRecordResponse>.Fail("An unexpected error occurred: " + ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ServiceRecordResponse>>> Update(
        int id,
        [FromBody] UpdateServiceRecordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var record = await _serviceRecordService.UpdateAsync(id, request, cancellationToken);
            return Ok(ApiResponse<ServiceRecordResponse>.Ok(record, "Service record updated successfully."));
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

    [HttpPost("{id:int}/invoice")]
    public async Task<ActionResult<ApiResponse<ServiceInvoiceResponse>>> GenerateInvoice(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _serviceInvoiceService.CreateAsync(id, cancellationToken);
            return Ok(ApiResponse<ServiceInvoiceResponse>.Ok(response, "Service invoice generated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ServiceInvoiceResponse>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ServiceInvoiceResponse>.Fail("An unexpected error occurred: " + ex.Message));
        }
    }
}
