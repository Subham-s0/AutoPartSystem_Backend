using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories
{
    public class PurchaseInvoiceRepository : IPurchaseInvoiceRepository
    {
        private readonly ApplicationDbContext _context;

        public PurchaseInvoiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PurchaseInvoice>> GetAllAsync()
        {
            return await _context.PurchaseInvoices
                .Include(i => i.Vendor)
                .Include(i => i.Items!)
                    .ThenInclude(it => it.Part)
                .ToListAsync();
        }

        public async Task<PurchaseInvoice?> GetByIdAsync(int id)
        {
            return await _context.PurchaseInvoices
                .Include(i => i.Vendor)
                .Include(i => i.Items!)
                    .ThenInclude(it => it.Part)
                .FirstOrDefaultAsync(i => i.PurchaseInvoiceId == id);
        }

        public async Task AddAsync(PurchaseInvoice invoice)
        {
            decimal subtotal = 0;

            foreach (var item in invoice.Items ?? new List<PurchaseInvoiceItem>())
            {
                item.LineTotal = item.Quantity * item.UnitCost;
                subtotal += item.LineTotal;

                var part = await _context.Parts.FindAsync(item.PartId);
                if (part != null)
                {
                    part.StockQty += item.Quantity;
                }
            }

            invoice.Subtotal = subtotal;
            invoice.TotalAmount = subtotal + invoice.TaxAmount - invoice.DiscountAmount;

            await _context.PurchaseInvoices.AddAsync(invoice);
            await _context.SaveChangesAsync();
        }
    }
}