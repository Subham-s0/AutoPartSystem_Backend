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
[Route("api/customer")]
public class CustomerPortalController : ControllerBase
{
    private readonly ICustomerPortalService _customerPortalService;

    public CustomerPortalController(ICustomerPortalService customerPortalService)
    {
        _customerPortalService = customerPortalService;
    }

    [HttpPost("appointments")]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> BookAppointment(
        BookAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _customerPortalService.BookAppointmentAsync(GetCurrentUserId(), request, cancellationToken);
            return Ok(ApiResponse<AppointmentResponse>.Ok(appointment, "Appointment booked successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AppointmentResponse>.Fail(ex.Message));
        }
    }

    [HttpGet("appointments")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<AppointmentResponse>>>> GetAppointments(CancellationToken cancellationToken)
    {
        var appointments = await _customerPortalService.GetAppointmentsAsync(GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<AppointmentResponse>>.Ok(appointments, "Appointments fetched successfully."));
    }

    [HttpPost("part-requests")]
    public async Task<ActionResult<ApiResponse<PartRequestResponse>>> CreatePartRequest(
        CreatePartRequestRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var partRequest = await _customerPortalService.CreatePartRequestAsync(GetCurrentUserId(), request, cancellationToken);
            return Ok(ApiResponse<PartRequestResponse>.Ok(partRequest, "Part request submitted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PartRequestResponse>.Fail(ex.Message));
        }
    }

    [HttpGet("part-requests")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<PartRequestResponse>>>> GetPartRequests(CancellationToken cancellationToken)
    {
        var partRequests = await _customerPortalService.GetPartRequestsAsync(GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<PartRequestResponse>>.Ok(partRequests, "Part requests fetched successfully."));
    }

    [HttpPost("reviews")]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> CreateReview(
        CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var review = await _customerPortalService.CreateReviewAsync(GetCurrentUserId(), request, cancellationToken);
            return Ok(ApiResponse<ReviewResponse>.Ok(review, "Review submitted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ReviewResponse>.Fail(ex.Message));
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<CustomerHistoryResponse>>> GetHistory(CancellationToken cancellationToken)
    {
        try
        {
            var history = await _customerPortalService.GetHistoryAsync(GetCurrentUserId(), cancellationToken);
            return Ok(ApiResponse<CustomerHistoryResponse>.Ok(history, "Customer history fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CustomerHistoryResponse>.Fail(ex.Message));
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
