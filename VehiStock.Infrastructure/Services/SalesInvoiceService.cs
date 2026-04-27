using VehiStock.Application.DTOs.SalesInvoices;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

// Implementation for invoice generation
public class SalesInvoiceService(ISalesInvoiceRepository salesInvoiceRepository) : ISalesInvoiceService
{
    public async Task<SalesInvoiceDto> CreateSalesInvoiceAsync(CreateSalesInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("At least one sales invoice item is required.");
        }

        if (await salesInvoiceRepository.SalesInvoiceExistsAsync(request.InvoiceNo, cancellationToken))
        {
            throw new InvalidOperationException("A sales invoice with this invoice number already exists.");
        }

        var customer = await salesInvoiceRepository.GetCustomerAsync(request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException("Customer was not found.");

        var vehicle = await salesInvoiceRepository.GetVehicleForCustomerAsync(request.VehicleId, request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException("Vehicle was not found for the selected customer.");

        var staffMember = await salesInvoiceRepository.GetStaffMemberAsync(request.StaffMemberId, cancellationToken)
            ?? throw new KeyNotFoundException("Staff member was not found.");

        var partIds = request.Items.Select(x => x.PartId).Distinct().ToList();
        var parts = await salesInvoiceRepository.GetPartsByIdsAsync(partIds, cancellationToken);

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

        Payment? payment = null;
        if (request.AmountPaid > 0)
        {
            payment = new Payment
            {
                SalesInvoice = salesInvoice,
                CustomerId = customer.CustomerId,
                ReceivedByStaffId = staffMember.StaffMemberId,
                PaymentDate = DateTime.UtcNow,
                PaymentType = request.PaymentType,
                Amount = request.AmountPaid
            };
        }

        return await salesInvoiceRepository.CreateSalesInvoiceAsync(salesInvoice, payment, responseItems, cancellationToken);
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
