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
[Route("api/customer/service-invoices")]
public class CustomerServiceInvoiceController : ControllerBase
{
    private readonly ICustomerServiceInvoiceService _customerServiceInvoiceService;
    private readonly IServiceInvoicePaymentService _serviceInvoicePaymentService;

    public CustomerServiceInvoiceController(
        ICustomerServiceInvoiceService customerServiceInvoiceService,
        IServiceInvoicePaymentService serviceInvoicePaymentService)
    {
        _customerServiceInvoiceService = customerServiceInvoiceService;
        _serviceInvoicePaymentService = serviceInvoicePaymentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ServiceInvoiceListResponse>>>> GetServiceInvoices(
        [FromQuery] ServiceInvoiceQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invoices = await _customerServiceInvoiceService.GetServiceInvoicesPageAsync(GetCurrentUserId(), request, cancellationToken);
            return Ok(ApiResponse<PaginatedResponse<ServiceInvoiceListResponse>>.Ok(invoices, "Service invoices fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PaginatedResponse<ServiceInvoiceListResponse>>.Fail(ex.Message));
        }
    }

    [HttpGet("{serviceInvoiceId:int}")]
    public async Task<ActionResult<ApiResponse<ServiceInvoiceListResponse>>> GetServiceInvoice(
        int serviceInvoiceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _customerServiceInvoiceService.GetServiceInvoiceDetailAsync(GetCurrentUserId(), serviceInvoiceId, cancellationToken);
            return Ok(ApiResponse<ServiceInvoiceListResponse>.Ok(invoice, "Service invoice fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<ServiceInvoiceListResponse>.Fail(ex.Message));
        }
    }

    [HttpPatch("{serviceInvoiceId:int}/loyalty")]
    public async Task<ActionResult<ApiResponse<ServiceInvoiceListResponse>>> SetServiceInvoiceLoyalty(
        int serviceInvoiceId,
        [FromBody] SetServiceInvoiceLoyaltyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _customerServiceInvoiceService.SetLoyaltyAsync(
                GetCurrentUserId(),
                serviceInvoiceId,
                request,
                cancellationToken);
            return Ok(ApiResponse<ServiceInvoiceListResponse>.Ok(invoice, "Service invoice loyalty updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ServiceInvoiceListResponse>.Fail(ex.Message));
        }
    }

    [HttpPost("{serviceInvoiceId:int}/payments/initiate")]
    public async Task<ActionResult<ApiResponse<InvoicePaymentInitiateResponse>>> InitiateServiceInvoicePayment(
        int serviceInvoiceId,
        [FromBody] InvoicePaymentInitiateRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var initiation = await _serviceInvoicePaymentService.InitiateAsync(GetCurrentUserId(), serviceInvoiceId, request, cancellationToken);
            return Ok(ApiResponse<InvoicePaymentInitiateResponse>.Ok(initiation, "Payment initiated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<InvoicePaymentInitiateResponse>.Fail(ex.Message));
        }
    }

    [HttpPost("payments/verify")]
    public async Task<ActionResult<ApiResponse<InvoicePaymentVerifyResponse>>> VerifyServiceInvoicePayment(
        [FromBody] InvoicePaymentVerifyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var verification = await _serviceInvoicePaymentService.VerifyAsync(GetCurrentUserId(), request, cancellationToken);
            return Ok(ApiResponse<InvoicePaymentVerifyResponse>.Ok(verification, "Payment verified successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<InvoicePaymentVerifyResponse>.Fail(ex.Message));
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
