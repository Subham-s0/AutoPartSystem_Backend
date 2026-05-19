using Microsoft.EntityFrameworkCore;
using VehiStock.Application.DTOs;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Application.Services
{
    public class PurchaseInvoiceService : IPurchaseInvoiceService
    {
        private readonly IPurchaseInvoiceRepository _repository;
        private readonly ApplicationDbContext _context;

        public PurchaseInvoiceService(
            IPurchaseInvoiceRepository repository,
            ApplicationDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<IEnumerable<PurchaseInvoiceDto>> GetAllAsync()
        {
            var invoices = await _repository.GetAllAsync();

            return invoices.Select(invoice => new PurchaseInvoiceDto
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
        }

        public async Task<PurchaseInvoiceDto?> GetByIdAsync(int id)
        {
            var invoice = await _repository.GetByIdAsync(id);

            if (invoice == null)
                return null;

            return new PurchaseInvoiceDto
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
        }

        public async Task<string> CreateAsync(CreatePurchaseInvoiceDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                return "At least one purchase invoice item is required.";

            var existingUserId = await _context.Users
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(existingUserId))
                return "Please register at least one user before creating purchase invoice.";

            var invoice = new PurchaseInvoice
            {
                VendorId = dto.VendorId,
                InvoiceNo = dto.InvoiceNo,
                PurchaseDate = dto.PurchaseDate,
                TaxAmount = dto.TaxAmount,
                DiscountAmount = dto.DiscountAmount,
                PaymentStatus = dto.PaymentStatus,
                Notes = dto.Notes,
                CreatedByUserId = existingUserId,

                Items = dto.Items.Select(item => new PurchaseInvoiceItem
                {
                    PartId = item.PartId,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    LineTotal = item.Quantity * item.UnitCost
                }).ToList()
            };

            await _repository.AddAsync(invoice);

            return "Purchase invoice created successfully.";
        }
    }
}