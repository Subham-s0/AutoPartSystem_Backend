using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Common;
using VehiStock.Application.DTOs.SalesInvoices;
using VehiStock.Application.Interfaces.IServices;

namespace VehiStock.Controllers;

// Used for sales invoice endpoints
[ApiController]
[Route("api/staff/sales-invoices")]
[Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Staff}")]
public class SalesInvoicesController(ISalesInvoiceService salesInvoiceService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SalesInvoiceDto>> CreateSalesInvoice(CreateSalesInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoice = await salesInvoiceService.CreateSalesInvoiceAsync(request, cancellationToken);
        return Ok(invoice);
    }
}
