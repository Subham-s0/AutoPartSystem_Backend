using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;
using VehiStock.Entities;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Staff)]
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

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ServiceRecordResponse>>>> GetServiceRecords(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _serviceRecordService.GetAllAsync(cancellationToken);
            return Ok(ApiResponse<IReadOnlyCollection<ServiceRecordResponse>>.Ok(response, "Service records retrieved successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IReadOnlyCollection<ServiceRecordResponse>>.Fail("An unexpected error occurred: " + ex.Message));
        }
    }

    [HttpGet("lookups")]
    public async Task<ActionResult<ApiResponse<SalesInvoiceLookupResponse>>> GetLookups(CancellationToken cancellationToken = default)
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

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ServiceRecordResponse>>> CreateServiceRecord(
        [FromBody] CreateServiceRecordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(ApiResponse<ServiceRecordResponse>.Fail("User not authenticated"));

            var response = await _serviceRecordService.CreateAsync(userId, request, cancellationToken);
            return Ok(ApiResponse<ServiceRecordResponse>.Ok(response, "Service record created successfully."));
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

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ServiceRecordResponse>>> UpdateServiceRecord(
        int id,
        [FromBody] UpdateServiceRecordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _serviceRecordService.UpdateAsync(id, request, cancellationToken);
            return Ok(ApiResponse<ServiceRecordResponse>.Ok(response, "Service record updated successfully."));
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
