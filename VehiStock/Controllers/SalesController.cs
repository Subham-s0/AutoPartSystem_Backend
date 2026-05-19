using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Staff)]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly ISalesInvoiceService _salesInvoiceService;

    public SalesController(ISalesInvoiceService salesInvoiceService)
    {
        _salesInvoiceService = salesInvoiceService;
    }

    [HttpPost("sell")]
    public async Task<ActionResult<ApiResponse<string>>> Sell(
        SellPartRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await _salesInvoiceService.SellPartAsync(GetCurrentUserId(), request, cancellationToken);
            return Ok(ApiResponse<string>.Ok(message, message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
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
