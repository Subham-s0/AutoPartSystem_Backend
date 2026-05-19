using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehiStock.Application.DTOs;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseInvoicesController : ControllerBase
    {
        private readonly IPurchaseInvoiceRepository _repository;
        private readonly ApplicationDbContext _context;

        public PurchaseInvoicesController(
            IPurchaseInvoiceRepository repository,
            ApplicationDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var invoices = await _repository.GetAllAsync();

            var result = invoices.Select(invoice => new PurchaseInvoiceDto
            {
                PurchaseInvoiceId = invoice.PurchaseInvoiceId,
                VendorId = invoice.VendorId,
                VendorName = invoice.Vendor != null ? invoice.Vendor.VendorName : null,
                InvoiceNo = invoice.InvoiceNo,
                PurchaseDate = invoice.PurchaseDate,
                Subtotal = invoice.Subtotal,
                TaxAmount = invoice.TaxAmount,
                DiscountAmount = invoice.DiscountAmount,
                TotalAmount = invoice.TotalAmount,
                PaymentStatus = invoice.PaymentStatus,
                Notes = invoice.Notes,
                CreatedByUserId = invoice.CreatedByUserId,

                Items = invoice.Items.Select(item => new PurchaseInvoiceItemDto
                {
                    PurchaseInvoiceItemId = item.PurchaseInvoiceItemId,
                    PartId = item.PartId,
                    PartName = item.Part != null ? item.Part.PartName : null,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    LineTotal = item.LineTotal
                }).ToList()
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _repository.GetByIdAsync(id);

            if (invoice == null)
                return NotFound("Purchase invoice not found.");

            var result = new PurchaseInvoiceDto
            {
                PurchaseInvoiceId = invoice.PurchaseInvoiceId,
                VendorId = invoice.VendorId,
                VendorName = invoice.Vendor != null ? invoice.Vendor.VendorName : null,
                InvoiceNo = invoice.InvoiceNo,
                PurchaseDate = invoice.PurchaseDate,
                Subtotal = invoice.Subtotal,
                TaxAmount = invoice.TaxAmount,
                DiscountAmount = invoice.DiscountAmount,
                TotalAmount = invoice.TotalAmount,
                PaymentStatus = invoice.PaymentStatus,
                Notes = invoice.Notes,
                CreatedByUserId = invoice.CreatedByUserId,

                Items = invoice.Items.Select(item => new PurchaseInvoiceItemDto
                {
                    PurchaseInvoiceItemId = item.PurchaseInvoiceItemId,
                    PartId = item.PartId,
                    PartName = item.Part != null ? item.Part.PartName : null,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    LineTotal = item.LineTotal
                }).ToList()
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePurchaseInvoiceDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                return BadRequest("At least one purchase invoice item is required.");

            var vendorExists = await _context.Vendors.AnyAsync(v => v.VendorId == dto.VendorId);

            if (!vendorExists)
                return BadRequest("Selected vendor does not exist.");

            var existingUserId = await _context.Users
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(existingUserId))
                return BadRequest("Please register at least one user first before creating purchase invoice.");

            decimal subtotal = 0;

            var invoiceItems = new List<PurchaseInvoiceItem>();

            foreach (var itemDto in dto.Items)
            {
                if (itemDto.Quantity <= 0)
                    return BadRequest("Quantity must be greater than 0.");

                if (itemDto.UnitCost <= 0)
                    return BadRequest("Unit cost must be greater than 0.");

                var part = await _context.Parts.FindAsync(itemDto.PartId);

                if (part == null)
                    return BadRequest($"Part with ID {itemDto.PartId} not found.");

                part.StockQty += itemDto.Quantity;
                part.UnitCost = itemDto.UnitCost;

                var lineTotal = itemDto.Quantity * itemDto.UnitCost;
                subtotal += lineTotal;

                invoiceItems.Add(new PurchaseInvoiceItem
                {
                    PartId = itemDto.PartId,
                    Quantity = itemDto.Quantity,
                    UnitCost = itemDto.UnitCost,
                    LineTotal = lineTotal
                });
            }

            var totalAmount = subtotal + dto.TaxAmount - dto.DiscountAmount;

            if (totalAmount < 0)
                return BadRequest("Total amount cannot be negative.");

            var invoice = new PurchaseInvoice
            {
                VendorId = dto.VendorId,
                InvoiceNo = dto.InvoiceNo,
                PurchaseDate = dto.PurchaseDate,
                Subtotal = subtotal,
                TaxAmount = dto.TaxAmount,
                DiscountAmount = dto.DiscountAmount,
                TotalAmount = totalAmount,
                PaymentStatus = dto.PaymentStatus,
                Notes = dto.Notes,
                CreatedByUserId = existingUserId,
                Items = invoiceItems
            };

            await _repository.AddAsync(invoice);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Purchase invoice created successfully and stock updated." });
        }
    }
}