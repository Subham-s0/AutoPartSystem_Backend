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
public class CustomerPaymentController : ControllerBase
{
    private readonly ICustomerPaymentService _customerPaymentService;
    private readonly ISalesInvoicePaymentService _salesInvoicePaymentService;

    public CustomerPaymentController(
        ICustomerPaymentService customerPaymentService,
        ISalesInvoicePaymentService salesInvoicePaymentService)
    {
        _customerPaymentService = customerPaymentService;
        _salesInvoicePaymentService = salesInvoicePaymentService;
    }

    [HttpGet("payments")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<CustomerPaymentListResponse>>>> GetPayments(
        [FromQuery] CustomerPaymentQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payments = await _customerPaymentService.GetPaymentsPageAsync(GetCurrentUserId(), request, cancellationToken);
            return Ok(ApiResponse<PaginatedResponse<CustomerPaymentListResponse>>.Ok(payments, "Payments fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PaginatedResponse<CustomerPaymentListResponse>>.Fail(ex.Message));
        }
    }

    [HttpPost("purchases/{salesInvoiceId:int}/payments/initiate")]
    public async Task<ActionResult<ApiResponse<InvoicePaymentInitiateResponse>>> InitiatePurchaseInvoicePayment(
        int salesInvoiceId,
        [FromBody] InvoicePaymentInitiateRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var initiation = await _salesInvoicePaymentService.InitiateAsync(GetCurrentUserId(), salesInvoiceId, request, cancellationToken);
            return Ok(ApiResponse<InvoicePaymentInitiateResponse>.Ok(initiation, "Payment initiated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<InvoicePaymentInitiateResponse>.Fail(ex.Message));
        }
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("Authenticated user ID is missing.");
        return userId;
    }
}
