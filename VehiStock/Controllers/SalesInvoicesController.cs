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
