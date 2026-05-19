using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

// Implementation for invoice generation
public class SalesInvoiceService : ISalesInvoiceService
{
    private readonly ISalesInvoiceRepository _salesInvoiceRepository;
    private readonly IEmailService _emailService;
    private readonly InvoiceTemplateService _invoiceTemplateService;

    public SalesInvoiceService(
        ISalesInvoiceRepository salesInvoiceRepository,
        IEmailService emailService,
        InvoiceTemplateService invoiceTemplateService)
    {
        _salesInvoiceRepository = salesInvoiceRepository;
        _emailService = emailService;
        _invoiceTemplateService = invoiceTemplateService;
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
                        .ToList()
                })
                .ToList(),
            Parts = parts
                .Select(part => new SalesInvoicePartLookupResponse
                {
                    PartId = part.PartId,
                    PartName = part.PartName,
                    Brand = part.Brand,
                    UnitPrice = part.UnitPrice,
                    StockQty = part.StockQty
                })
                .ToList()
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

        var partIds = request.Items.Select(x => x.PartId).Distinct().ToList();
        var parts = (await _salesInvoiceRepository.GetPartsByIdsAsync(partIds, cancellationToken)).ToDictionary(x => x.PartId);
        if (parts.Count != partIds.Count)
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
                PaymentType = request.PaymentType,
                Amount = request.AmountPaid
            };
        }

        var created = await _salesInvoiceRepository.CreateSalesInvoiceAsync(salesInvoice, payment, cancellationToken);

        created.Customer = customer;
        created.Vehicle = vehicle;
        created.StaffMember = staffProfile;
        created.Items = invoiceItems;

        return MapResponse(created, responseItems);
    }

    public async Task<PaginatedResponse<SalesInvoiceResponse>> GetPaginatedAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedPageNumber = Math.Max(1, pageNumber);
        var normalizedPageSize = Math.Clamp(pageSize, 1, 100);
        var (items, totalRecords) = await _salesInvoiceRepository.GetPaginatedAsync(search, normalizedPageNumber, normalizedPageSize, cancellationToken);

        return new PaginatedResponse<SalesInvoiceResponse>
        {
            Items = items.Select(x => MapResponse(x)).ToList(),
            PageNumber = normalizedPageNumber,
            PageSize = normalizedPageSize,
            TotalRecords = totalRecords,
            TotalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)normalizedPageSize)
        };
    }

    public async Task<SalesInvoiceResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await _salesInvoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice is null)
        {
            throw new InvalidOperationException("Sales invoice was not found.");
        }

        return MapResponse(invoice);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await _salesInvoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice is null)
        {
            throw new InvalidOperationException("Sales invoice was not found.");
        }

        await _salesInvoiceRepository.DeleteAsync(invoice, cancellationToken);
    }

    public async Task SendEmailAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await _salesInvoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice is null)
        {
            throw new InvalidOperationException("Sales invoice was not found.");
        }

        var customerEmail = invoice.Customer.User.Email;
        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            throw new InvalidOperationException("Customer email is not available for this invoice.");
        }

        var htmlBody = _invoiceTemplateService.Generate(
            invoice.Customer.User.FullName,
            invoice.InvoiceNo,
            invoice.TotalAmount);

        await _emailService.SendInvoiceEmail(customerEmail, "VehiStock Invoice", htmlBody);
        invoice.EmailSentAt = DateTime.UtcNow;
        await _salesInvoiceRepository.SaveChangesAsync(cancellationToken);
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

    private static SalesInvoiceResponse MapResponse(SalesInvoice invoice, IReadOnlyCollection<SalesInvoiceItemResponse>? itemsOverride = null)
    {
        return new SalesInvoiceResponse
        {
            SalesInvoiceId = invoice.SalesInvoiceId,
            InvoiceNo = invoice.InvoiceNo,
            CustomerId = invoice.CustomerId,
            CustomerName = invoice.Customer?.User?.FullName,
            VehicleId = invoice.VehicleId,
            VehicleNumber = invoice.Vehicle?.VehicleNumber,
            StaffMemberId = invoice.StaffMemberId,
            StaffName = invoice.StaffMember?.User?.FullName,
            InvoiceDate = invoice.InvoiceDate,
            Subtotal = invoice.Subtotal,
            DiscountPercent = invoice.DiscountPercent,
            DiscountAmount = invoice.DiscountAmount,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            AmountPaid = invoice.AmountPaid,
            BalanceDue = invoice.BalanceDue,
            CreditDueDate = invoice.CreditDueDate,
            PaymentType = invoice.PaymentType.ToString(),
            PaymentStatus = invoice.PaymentStatus.ToString(),
            Items = itemsOverride ?? invoice.Items.Select(item => new SalesInvoiceItemResponse
            {
                PartId = item.PartId,
                PartName = item.Part?.PartName ?? string.Empty,
                Brand = item.Part?.Brand ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                LineTotal = item.LineTotal
            }).ToList()
        };
    }
}
