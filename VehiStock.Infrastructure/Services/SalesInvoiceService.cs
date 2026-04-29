using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

// Implementation for invoice generation
public class SalesInvoiceService : ISalesInvoiceService
{
    private readonly ISalesInvoiceRepository _salesInvoiceRepository;

    public SalesInvoiceService(ISalesInvoiceRepository salesInvoiceRepository)
    {
        _salesInvoiceRepository = salesInvoiceRepository;
    }

    public async Task<SalesInvoiceLookupResponse> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _salesInvoiceRepository.GetCustomersWithVehiclesAsync(cancellationToken);
        var parts = await _salesInvoiceRepository.GetAvailablePartsAsync(cancellationToken);

        return new SalesInvoiceLookupResponse
        {
            Customers = customers
                .Select(customer => new SalesInvoiceCustomerLookupResponse
                {
                    CustomerId = customer.CustomerId,
                    FullName = customer.User.FullName,
                    Email = customer.User.Email ?? string.Empty,
                    PhoneNumber = customer.User.PhoneNumber,
                    Vehicles = customer.Vehicles
                        .OrderBy(vehicle => vehicle.VehicleNumber)
                        .Select(vehicle => new SalesInvoiceVehicleLookupResponse
                        {
                            VehicleId = vehicle.VehicleId,
                            VehicleNumber = vehicle.VehicleNumber,
                            Make = vehicle.Make,
                            Model = vehicle.Model
                        })
                        .ToArray()
                })
                .ToArray(),
            Parts = parts
                .Select(part => new SalesInvoicePartLookupResponse
                {
                    PartId = part.PartId,
                    PartName = part.PartName,
                    Brand = part.Brand,
                    UnitPrice = part.UnitPrice,
                    StockQty = part.StockQty
                })
                .ToArray()
        };
    }

    public async Task<SalesInvoiceResponse> CreateAsync(string userId, CreateSalesInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("At least one item is required.");
        }

        var staffProfile = await _salesInvoiceRepository.GetStaffProfileByUserIdAsync(userId, cancellationToken);
        if (staffProfile is null)
        {
            throw new InvalidOperationException("Staff profile was not found for this account.");
        }

        var invoiceNo = await GenerateInvoiceNoAsync(cancellationToken);

        var customer = await _salesInvoiceRepository.GetCustomerAsync(request.CustomerId, cancellationToken);
        if (customer is null)
        {
            throw new InvalidOperationException("Customer was not found.");
        }

        var vehicle = await _salesInvoiceRepository.GetVehicleForCustomerAsync(request.CustomerId, request.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            throw new InvalidOperationException("Vehicle was not found for this customer.");
        }

        var partIds = request.Items.Select(x => x.PartId).Distinct().ToArray();
        var parts = (await _salesInvoiceRepository.GetPartsByIdsAsync(partIds, cancellationToken)).ToDictionary(x => x.PartId);
        if (parts.Count != partIds.Length)
        {
            throw new InvalidOperationException("One or more parts were not found.");
        }

        var invoiceItems = new List<SalesInvoiceItem>();
        var responseItems = new List<SalesInvoiceItemResponse>();
        decimal subtotal = 0m;
        decimal lineDiscountTotal = 0m;

        foreach (var item in request.Items)
        {
            var part = parts[item.PartId];
            var gross = part.UnitPrice * item.Quantity;
            if (item.DiscountAmount > gross)
            {
                throw new InvalidOperationException($"Discount cannot exceed line amount for part {part.PartName}.");
            }

            part.DecreaseStock(item.Quantity);

            var lineTotal = gross - item.DiscountAmount;
            subtotal += gross;
            lineDiscountTotal += item.DiscountAmount;

            invoiceItems.Add(new SalesInvoiceItem
            {
                PartId = part.PartId,
                Quantity = item.Quantity,
                UnitPrice = part.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                LineTotal = lineTotal
            });

            responseItems.Add(new SalesInvoiceItemResponse
            {
                PartId = part.PartId,
                PartName = part.PartName,
                Brand = part.Brand,
                Quantity = item.Quantity,
                UnitPrice = part.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                LineTotal = lineTotal
            });
        }

        var invoiceDiscount = Math.Round(subtotal * (request.DiscountPercent / 100m), 2, MidpointRounding.AwayFromZero);
        var discountAmount = lineDiscountTotal + invoiceDiscount;
        var totalAmount = subtotal - discountAmount + request.TaxAmount;
        if (totalAmount < 0m)
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
            InvoiceNo = invoiceNo,
            CustomerId = customer.CustomerId,
            VehicleId = vehicle.VehicleId,
            StaffMemberId = staffProfile.StaffMemberId,
            InvoiceDate = request.InvoiceDate,
            Subtotal = subtotal,
            DiscountPercent = request.DiscountPercent,
            DiscountAmount = discountAmount,
            TaxAmount = request.TaxAmount,
            TotalAmount = totalAmount,
            AmountPaid = request.AmountPaid,
            BalanceDue = balanceDue,
            CreditDueDate = request.CreditDueDate,
            PaymentType = request.PaymentType,
            PaymentStatus = paymentStatus,
            Items = invoiceItems
        };

        Payment? payment = null;
        if (request.AmountPaid > 0m)
        {
            payment = new Payment
            {
                SalesInvoice = salesInvoice,
                CustomerId = customer.CustomerId,
                ReceivedByStaffId = staffProfile.StaffMemberId,
                PaymentType = request.PaymentType,
                Amount = request.AmountPaid
            };
        }

        var created = await _salesInvoiceRepository.CreateSalesInvoiceAsync(salesInvoice, payment, cancellationToken);

        return new SalesInvoiceResponse
        {
            SalesInvoiceId = created.SalesInvoiceId,
            InvoiceNo = created.InvoiceNo,
            CustomerId = created.CustomerId,
            VehicleId = created.VehicleId,
            StaffMemberId = created.StaffMemberId,
            InvoiceDate = created.InvoiceDate,
            Subtotal = created.Subtotal,
            DiscountPercent = created.DiscountPercent,
            DiscountAmount = created.DiscountAmount,
            TaxAmount = created.TaxAmount,
            TotalAmount = created.TotalAmount,
            AmountPaid = created.AmountPaid,
            BalanceDue = created.BalanceDue,
            CreditDueDate = created.CreditDueDate,
            PaymentType = created.PaymentType,
            PaymentStatus = created.PaymentStatus,
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

    private async Task<string> GenerateInvoiceNoAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = $"SI-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            if (attempt > 0)
            {
                candidate = $"{candidate}-{attempt}";
            }

            if (!await _salesInvoiceRepository.InvoiceExistsAsync(candidate, cancellationToken))
            {
                return candidate;
            }

            await Task.Delay(5, cancellationToken);
        }

        throw new InvalidOperationException("Unable to generate a unique invoice number.");
    }
}
