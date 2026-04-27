using Microsoft.EntityFrameworkCore;
using VehiStock.Application.DTOs.SalesInvoices;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

// Implementation for invoice generation data access
public class SalesInvoiceRepository(ApplicationDbContext dbContext) : ISalesInvoiceRepository
{
    public Task<bool> SalesInvoiceExistsAsync(string invoiceNo, CancellationToken cancellationToken = default) =>
        dbContext.SalesInvoices.AnyAsync(x => x.InvoiceNo == invoiceNo, cancellationToken);

    public Task<CustomerProfile?> GetCustomerAsync(int customerId, CancellationToken cancellationToken = default) =>
        dbContext.CustomerProfiles.FirstOrDefaultAsync(x => x.CustomerId == customerId, cancellationToken);

    public Task<Vehicle?> GetVehicleForCustomerAsync(int vehicleId, int customerId, CancellationToken cancellationToken = default) =>
        dbContext.Vehicles.FirstOrDefaultAsync(x => x.VehicleId == vehicleId && x.CustomerId == customerId, cancellationToken);

    public Task<StaffProfile?> GetStaffMemberAsync(int staffMemberId, CancellationToken cancellationToken = default) =>
        dbContext.StaffProfiles.FirstOrDefaultAsync(x => x.StaffMemberId == staffMemberId, cancellationToken);

    public Task<Dictionary<int, Part>> GetPartsByIdsAsync(IReadOnlyCollection<int> partIds, CancellationToken cancellationToken = default) =>
        dbContext.Parts.Where(x => partIds.Contains(x.PartId)).ToDictionaryAsync(x => x.PartId, cancellationToken);

    public async Task<SalesInvoiceDto> CreateSalesInvoiceAsync(
        SalesInvoice salesInvoice,
        Payment? payment,
        IReadOnlyList<SalesInvoiceItemDto> responseItems,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.SalesInvoices.Add(salesInvoice);

        if (payment is not null)
        {
            dbContext.Payments.Add(payment);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new SalesInvoiceDto
        {
            SalesInvoiceId = salesInvoice.SalesInvoiceId,
            InvoiceNo = salesInvoice.InvoiceNo,
            CustomerId = salesInvoice.CustomerId,
            VehicleId = salesInvoice.VehicleId,
            StaffMemberId = salesInvoice.StaffMemberId,
            InvoiceDate = salesInvoice.InvoiceDate,
            Subtotal = salesInvoice.Subtotal,
            DiscountPercent = salesInvoice.DiscountPercent,
            DiscountAmount = salesInvoice.DiscountAmount,
            TaxAmount = salesInvoice.TaxAmount,
            TotalAmount = salesInvoice.TotalAmount,
            AmountPaid = salesInvoice.AmountPaid,
            BalanceDue = salesInvoice.BalanceDue,
            CreditDueDate = salesInvoice.CreditDueDate,
            PaymentType = salesInvoice.PaymentType,
            PaymentStatus = salesInvoice.PaymentStatus,
            Items = responseItems.ToList()
        };
    }
}
