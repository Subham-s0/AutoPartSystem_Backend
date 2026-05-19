using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;
using VehiStock.Infrastructure.Persistance;
using VehiStock.Infrastructure.Services;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Staff)]
[Route("api/staff/sales-invoices")]
public class SalesInvoicesController : ControllerBase
{
    private readonly ISalesInvoiceService _salesInvoiceService;
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly InvoiceTemplateService _templateService;

    public SalesInvoicesController(
        ISalesInvoiceService salesInvoiceService,
        ApplicationDbContext context,
        IEmailService emailService,
        InvoiceTemplateService templateService)
    {
        _salesInvoiceService = salesInvoiceService;
        _context = context;
        _emailService = emailService;
        _templateService = templateService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<SalesInvoiceResponse>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _salesInvoiceService.GetPagedSalesInvoicesAsync(search, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PaginatedResponse<SalesInvoiceResponse>>.Ok(response, "Sales invoices retrieved successfully."));
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

    [HttpPost("{invoiceId:int}/send-email")]
    public async Task<ActionResult<ApiResponse<object>>> SendEmail(
        int invoiceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var salesInvoice = await _context.SalesInvoices
                .Include(x => x.Customer).ThenInclude(c => c.User)
                .Include(x => x.Items).ThenInclude(i => i.Part)
                .FirstOrDefaultAsync(x => x.SalesInvoiceId == invoiceId, cancellationToken);

            if (salesInvoice == null)
            {
                return NotFound(ApiResponse<object>.Fail("Sales invoice not found."));
            }

            var customerEmail = salesInvoice.Customer?.User?.Email;
            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                return BadRequest(ApiResponse<object>.Fail("Customer email address is not registered."));
            }

            var customerName = salesInvoice.Customer?.User?.FullName ?? "Customer";
            var htmlContent = _templateService.Generate(salesInvoice, customerName);

            await _emailService.SendInvoiceEmail(customerEmail, $"VehiStock Invoice - {salesInvoice.InvoiceNo}", htmlContent);

            return Ok(ApiResponse<object>.Ok(null!, "Invoice email sent successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail("Failed to send email: " + ex.Message));
        }
    }

    [HttpDelete("{invoiceId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        int invoiceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var salesInvoice = await _context.SalesInvoices
                .Include(x => x.Items).ThenInclude(i => i.Part)
                .FirstOrDefaultAsync(x => x.SalesInvoiceId == invoiceId, cancellationToken);

            if (salesInvoice == null)
            {
                return NotFound(ApiResponse<object>.Fail("Sales invoice not found."));
            }

            // Restore part stocks
            foreach (var item in salesInvoice.Items)
            {
                if (item.Part != null)
                {
                    item.Part.IncreaseStock(item.Quantity);
                }
            }

            // Remove associated payments
            var payments = await _context.Payments
                .Where(p => p.SalesInvoiceId == invoiceId)
                .ToListAsync(cancellationToken);

            if (payments.Any())
            {
                _context.Payments.RemoveRange(payments);
            }

            _context.SalesInvoices.Remove(salesInvoice);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null!, "Sales invoice deleted successfully and stock restored."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail("Failed to delete invoice: " + ex.Message));
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
