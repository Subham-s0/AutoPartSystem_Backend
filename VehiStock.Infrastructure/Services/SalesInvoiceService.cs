using Microsoft.EntityFrameworkCore;
using VehiStock.Application.DTOs.SalesInvoices;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Services;

public class SalesInvoiceService(ApplicationDbContext dbContext) : ISalesInvoiceService
{
    public async Task<SalesInvoiceDto> CreateSalesInvoiceAsync(CreateSalesInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("At least one sales invoice item is required.");
        }

        var duplicateInvoice = await dbContext.SalesInvoices
            .AnyAsync(x => x.InvoiceNo == request.InvoiceNo, cancellationToken);

        if (duplicateInvoice)
        {
            throw new InvalidOperationException("A sales invoice with this invoice number already exists.");
        }

        var customer = await dbContext.CustomerProfiles
            .FirstOrDefaultAsync(x => x.CustomerId == request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException("Customer was not found.");

        var vehicle = await dbContext.Vehicles
            .FirstOrDefaultAsync(x => x.VehicleId == request.VehicleId && x.CustomerId == request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException("Vehicle was not found for the selected customer.");

        var staffMember = await dbContext.StaffProfiles
            .FirstOrDefaultAsync(x => x.StaffMemberId == request.StaffMemberId, cancellationToken)
            ?? throw new KeyNotFoundException("Staff member was not found.");

        var partIds = request.Items.Select(x => x.PartId).Distinct().ToList();
        var parts = await dbContext.Parts
            .Where(x => partIds.Contains(x.PartId))
            .ToDictionaryAsync(x => x.PartId, cancellationToken);

        if (parts.Count != partIds.Count)
        {
            throw new KeyNotFoundException("One or more parts were not found.");
        }

        var lineItems = new List<SalesInvoiceItem>();
        var responseItems = new List<SalesInvoiceItemDto>();
        decimal subtotal = 0m;
        decimal totalLineDiscount = 0m;

        foreach (var item in request.Items)
        {
            var part = parts[item.PartId];
            var grossLineAmount = part.UnitPrice * item.Quantity;

            if (item.DiscountAmount > grossLineAmount)
            {
                throw new InvalidOperationException($"Discount cannot exceed line amount for part {part.PartCode}.");
            }

            part.DecreaseStock(item.Quantity);

            var lineTotal = grossLineAmount - item.DiscountAmount;
            subtotal += grossLineAmount;
            totalLineDiscount += item.DiscountAmount;

            lineItems.Add(new SalesInvoiceItem
            {
                PartId = part.PartId,
                Quantity = item.Quantity,
                UnitPrice = part.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                LineTotal = lineTotal
            });

            responseItems.Add(new SalesInvoiceItemDto
            {
                PartId = part.PartId,
                PartName = part.PartName,
                Quantity = item.Quantity,
                UnitPrice = part.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                LineTotal = lineTotal
            });
        }

        var invoiceLevelDiscount = Math.Round(subtotal * (request.DiscountPercent / 100m), 2, MidpointRounding.AwayFromZero);
        var totalDiscount = totalLineDiscount + invoiceLevelDiscount;
        var totalAmount = subtotal - totalDiscount + request.TaxAmount;

        if (totalAmount < 0)
        {
            throw new InvalidOperationException("Total amount cannot be negative.");
        }

        if (request.AmountPaid > totalAmount)
        {
            throw new InvalidOperationException("Amount paid cannot exceed total amount.");
        }

        var balanceDue = totalAmount - request.AmountPaid;
        var paymentStatus = ResolvePaymentStatus(totalAmount, request.AmountPaid);

        var salesInvoice = new SalesInvoice
        {
            InvoiceNo = request.InvoiceNo.Trim(),
            CustomerId = customer.CustomerId,
            VehicleId = vehicle.VehicleId,
            StaffMemberId = staffMember.StaffMemberId,
            InvoiceDate = request.InvoiceDate,
            Subtotal = subtotal,
            DiscountPercent = request.DiscountPercent,
            DiscountAmount = totalDiscount,
            TaxAmount = request.TaxAmount,
            TotalAmount = totalAmount,
            AmountPaid = request.AmountPaid,
            BalanceDue = balanceDue,
            CreditDueDate = request.CreditDueDate,
            PaymentType = request.PaymentType,
            PaymentStatus = paymentStatus,
            Items = lineItems
        };

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.SalesInvoices.Add(salesInvoice);

        if (request.AmountPaid > 0)
        {
            dbContext.Payments.Add(new Payment
            {
                SalesInvoice = salesInvoice,
                CustomerId = customer.CustomerId,
                ReceivedByStaffId = staffMember.StaffMemberId,
                PaymentDate = DateTime.UtcNow,
                PaymentType = request.PaymentType,
                Amount = request.AmountPaid
            });
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
            Items = responseItems
        };
    }

    private static PaymentStatus ResolvePaymentStatus(decimal totalAmount, decimal amountPaid)
    {
        if (totalAmount == 0m || amountPaid == totalAmount)
        {
            return PaymentStatus.Paid;
        }

        if (amountPaid == 0m)
        {
            return PaymentStatus.Unpaid;
        }

        return PaymentStatus.Partial;
    }
}
