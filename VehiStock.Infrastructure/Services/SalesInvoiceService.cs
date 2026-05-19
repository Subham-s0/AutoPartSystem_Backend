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
            throw new InvalidOperationException("One or more selected parts could not be found.");
        }

        var salesInvoice = new SalesInvoice
        {
            InvoiceNo = invoiceNo,
            CustomerId = request.CustomerId,
            VehicleId = request.VehicleId,
            StaffMemberId = staffProfile.StaffMemberId,
            InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Subtotal = 0m,
            DiscountPercent = request.DiscountPercent,
            DiscountAmount = 0m,
            TaxAmount = 0m,
            TotalAmount = 0m,
            AmountPaid = request.AmountPaid,
            BalanceDue = 0m,
            CreditDueDate = request.CreditDueDate,
            PaymentType = request.PaymentType
        };

        var items = new List<SalesInvoiceItem>();
        var responseItems = new List<SalesInvoiceItemResponse>();

        foreach (var itemDraft in request.Items)
        {
            var part = parts[itemDraft.PartId];
            if (part.StockQty < itemDraft.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for part: {part.PartName} (Available: {part.StockQty}, Requested: {itemDraft.Quantity})");
            }

            part.DecreaseStock(itemDraft.Quantity);

            var lineSubtotal = part.UnitPrice * itemDraft.Quantity;
            var lineDiscount = itemDraft.DiscountAmount;
            var lineTotal = lineSubtotal - lineDiscount;

            salesInvoice.Subtotal += lineSubtotal;
            salesInvoice.DiscountAmount += lineDiscount;

            var invoiceItem = new SalesInvoiceItem
            {
                PartId = itemDraft.PartId,
                Quantity = itemDraft.Quantity,
                UnitPrice = part.UnitPrice,
                DiscountAmount = lineDiscount,
                LineTotal = lineTotal
            };

            items.Add(invoiceItem);

            responseItems.Add(new SalesInvoiceItemResponse
            {
                PartId = invoiceItem.PartId,
                PartName = part.PartName,
                Brand = part.Brand,
                Quantity = invoiceItem.Quantity,
                UnitPrice = invoiceItem.UnitPrice,
                DiscountAmount = invoiceItem.DiscountAmount,
                LineTotal = invoiceItem.LineTotal
            });
        }

        salesInvoice.Items = items;

        var mainDiscount = (salesInvoice.Subtotal - salesInvoice.DiscountAmount) * (salesInvoice.DiscountPercent / 100m);
        salesInvoice.DiscountAmount += mainDiscount;

        var taxableAmount = salesInvoice.Subtotal - salesInvoice.DiscountAmount;
        salesInvoice.TaxAmount = taxableAmount * 0.13m; // 13% VAT
        salesInvoice.TotalAmount = taxableAmount + salesInvoice.TaxAmount;

        salesInvoice.PaymentStatus = ResolvePaymentStatus(salesInvoice.TotalAmount, salesInvoice.AmountPaid);
        salesInvoice.BalanceDue = salesInvoice.TotalAmount - salesInvoice.AmountPaid;

        Payment? payment = null;
        if (salesInvoice.AmountPaid > 0m)
        {
            payment = new Payment
            {
                CustomerId = salesInvoice.CustomerId,
                Amount = salesInvoice.AmountPaid,
                PaymentDate = DateTime.UtcNow,
                PaymentType = salesInvoice.PaymentType,
                Notes = $"Initial payment for Invoice {salesInvoice.InvoiceNo}"
            };
            salesInvoice.Payments.Add(payment);
        }

        var created = await _salesInvoiceRepository.CreateSalesInvoiceAsync(salesInvoice, payment, cancellationToken);
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

    public async Task<IReadOnlyCollection<SalesInvoiceResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _salesInvoiceRepository.GetSalesInvoicesAsync(cancellationToken);
        return invoices.Select(x => MapResponse(x)).ToList();
    }

    public async Task SendEmailAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await _salesInvoiceRepository.GetSalesInvoiceByIdAsync(id, cancellationToken);
        if (invoice is null)
        {
            throw new InvalidOperationException("Sales invoice not found.");
        }

        var customerEmail = invoice.Customer?.User?.Email;
        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            throw new InvalidOperationException("Customer email address is not available.");
        }

        var customerName = invoice.Customer?.User?.FullName ?? "Customer";

        var itemsHtml = string.Join("", invoice.Items.Select(item => $@"
            <tr>
                <td style='padding: 10px; border-bottom: 1px solid #e2e8f0; color: #334155;'>{item.Part?.PartName ?? "Unknown Part"}</td>
                <td style='padding: 10px; border-bottom: 1px solid #e2e8f0; text-align: center; color: #334155;'>{item.Quantity}</td>
                <td style='padding: 10px; border-bottom: 1px solid #e2e8f0; text-align: right; color: #334155;'>NPR {item.UnitPrice:N2}</td>
                <td style='padding: 10px; border-bottom: 1px solid #e2e8f0; text-align: right; color: #334155;'>NPR {item.LineTotal:N2}</td>
            </tr>
        "));

        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.05);'>
                <div style='background-color: #059669; color: white; padding: 25px 20px; text-align: center;'>
                    <h1 style='margin: 0; font-size: 26px; font-weight: 700; letter-spacing: 0.5px;'>VehiAutoPart</h1>
                    <p style='margin: 5px 0 0 0; opacity: 0.9; font-size: 14px;'>Invoice #{invoice.InvoiceNo}</p>
                </div>
                
                <div style='padding: 30px; background-color: #ffffff;'>
                    <p style='font-size: 16px; color: #334155; margin-top: 0;'>Hello <strong>{customerName}</strong>,</p>
                    <p style='font-size: 16px; color: #334155; line-height: 1.5;'>Thank you for your business. Here are the details of your recent purchase on {invoice.InvoiceDate.ToString("MMMM dd, yyyy")}:</p>
                    
                    <table style='width: 100%; border-collapse: collapse; margin-top: 25px; margin-bottom: 25px;'>
                        <thead>
                            <tr style='background-color: #f8fafc; text-align: left;'>
                                <th style='padding: 12px 10px; border-bottom: 2px solid #cbd5e1; color: #475569; font-size: 14px;'>Part</th>
                                <th style='padding: 12px 10px; border-bottom: 2px solid #cbd5e1; text-align: center; color: #475569; font-size: 14px;'>Qty</th>
                                <th style='padding: 12px 10px; border-bottom: 2px solid #cbd5e1; text-align: right; color: #475569; font-size: 14px;'>Unit Price</th>
                                <th style='padding: 12px 10px; border-bottom: 2px solid #cbd5e1; text-align: right; color: #475569; font-size: 14px;'>Total</th>
                            </tr>
                        </thead>
                        <tbody>
                            {itemsHtml}
                        </tbody>
                    </table>

                    <div style='text-align: right; font-size: 15px; color: #334155; border-top: 1px solid #f1f5f9; padding-top: 15px;'>
                        <p style='margin: 5px 0;'><strong>Subtotal:</strong> NPR {invoice.Subtotal:N2}</p>
                        <p style='margin: 5px 0;'><strong>Tax:</strong> NPR {invoice.TaxAmount:N2}</p>
                        <p style='margin: 10px 0 0 0; font-size: 18px; color: #059669;'><strong>Total Amount:</strong> NPR {invoice.TotalAmount:N2}</p>
                    </div>

                    <p style='font-size: 14px; color: #64748b; text-align: center; margin-top: 35px; padding-top: 20px; border-top: 1px solid #f1f5f9; line-height: 1.5;'>
                        If you have any questions, please reply to this email or contact us at <a href='mailto:vehistock@gmail.com' style='color: #059669; text-decoration: none; font-weight: 500;'>vehistock@gmail.com</a>.
                    </p>
                </div>
            </div>
        ";

        var subject = $"Your Invoice from VehiAutoPart ({invoice.InvoiceNo})";
        
        await _emailService.SendInvoiceEmail(customerEmail, subject, htmlBody);
        
        // Update EmailSentAt in the database
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
            CustomerName = invoice.Customer?.User?.FullName ?? string.Empty,
            VehicleId = invoice.VehicleId,
            VehicleNumber = invoice.Vehicle?.VehicleNumber ?? string.Empty,
            StaffMemberId = invoice.StaffMemberId,
            StaffName = invoice.StaffMember?.User?.FullName ?? string.Empty,
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
            }).ToList() ?? []
        };
    }
}
