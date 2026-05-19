using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers;

// Used for sales invoice endpoints
[ApiController]
[Authorize(Roles = RoleNames.Staff)]
[Route("api/staff/sales-invoices")]
public class SalesInvoicesController : ControllerBase
{
    private readonly ISalesInvoiceService _salesInvoiceService;

    public SalesInvoicesController(ISalesInvoiceService salesInvoiceService)
    {
        _salesInvoiceService = salesInvoiceService;
    }

    [HttpGet("lookups")]
    public async Task<ActionResult<ApiResponse<SalesInvoiceLookupResponse>>> GetLookups(
        CancellationToken cancellationToken = default)
    {
        var response = await _salesInvoiceService.GetLookupAsync(cancellationToken);
        return Ok(ApiResponse<SalesInvoiceLookupResponse>.Ok(response, "Sales invoice lookups fetched successfully."));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SalesInvoiceResponse>>> Create(
        CreateSalesInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _salesInvoiceService.CreateAsync(GetCurrentUserId(), request, cancellationToken);
            return Ok(ApiResponse<SalesInvoiceResponse>.Ok(response, "Sales invoice created successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SalesInvoiceResponse>.Fail(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<SalesInvoiceResponse>>>> GetList(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _salesInvoiceService.GetPaginatedAsync(search, pageNumber, pageSize, cancellationToken);
            return Ok(ApiResponse<PaginatedResponse<SalesInvoiceResponse>>.Ok(response, "Sales invoices fetched successfully."));
        }
        catch (Exception ex)
        {
            Console.WriteLine("CRITICAL ERROR IN GET_LIST: " + ex.ToString());
            return StatusCode(500, ApiResponse<PaginatedResponse<SalesInvoiceResponse>>.Fail("Error: " + ex.Message));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<SalesInvoiceResponse>>> GetById(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _salesInvoiceService.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<SalesInvoiceResponse>.Ok(response, "Sales invoice fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<SalesInvoiceResponse>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _salesInvoiceService.DeleteAsync(id, cancellationToken);
            return Ok(ApiResponse<string>.Ok(string.Empty, "Sales invoice deleted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpPost("{id:int}/send-email")]
    public async Task<ActionResult<ApiResponse<string>>> SendEmail(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _salesInvoiceService.SendEmailAsync(id, cancellationToken);
            return Ok(ApiResponse<string>.Ok(string.Empty, "Sales invoice email sent successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail("An error occurred while sending the email. " + ex.Message));
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
