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

    public async Task<string> SellPartAsync(
        string userId,
        SellPartRequest request,
        CancellationToken cancellationToken = default)
    {
        var parts = await _salesInvoiceRepository.GetPartsByIdsAsync([request.PartId], cancellationToken);
        if (parts.Count == 0)
        {
            throw new InvalidOperationException("Part was not found.");
        }

        var part = parts.First();
        if (part.StockQty < request.Quantity)
        {
            throw new InvalidOperationException($"Insufficient stock for {part.PartName}.");
        }

        var lineTotal = part.UnitPrice * request.Quantity;
        var invoiceRequest = new CreateSalesInvoiceRequest
        {
            CustomerId = request.CustomerId,
            VehicleId = request.VehicleId,
            InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DiscountPercent = 0m,
            TaxAmount = 0m,
            AmountPaid = lineTotal,
            PaymentType = PaymentType.Khalti,
            Items =
            [
                new CreateSalesInvoiceItemRequest
                {
                    PartId = request.PartId,
                    Quantity = request.Quantity,
                    DiscountAmount = 0m
                }
            ]
        };

        await CreateAsync(userId, invoiceRequest, cancellationToken);
        return "Sale completed successfully";
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

    public async Task<SalesInvoiceResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await _salesInvoiceRepository.GetSalesInvoiceByIdAsync(id, cancellationToken);
        if (invoice is null)
        {
            throw new InvalidOperationException("Sales invoice not found.");
        }

        return MapResponse(invoice);
    }

    public async Task<IReadOnlyCollection<SalesInvoiceResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _salesInvoiceRepository.GetSalesInvoicesAsync(cancellationToken);
        return invoices.Select(x => MapResponse(x)).ToList();
    }

    public async Task<PaginatedResponse<SalesInvoiceResponse>> GetPaginatedAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await _salesInvoiceRepository.GetSalesInvoicesPaginatedAsync(search, pageNumber, pageSize, cancellationToken);
        
        var mappedItems = result.Items.Select(x => MapResponse(x)).ToList();

        return new PaginatedResponse<SalesInvoiceResponse>
        {
            Items = mappedItems,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages
        };
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await _salesInvoiceRepository.GetSalesInvoiceByIdAsync(id, cancellationToken);
        if (invoice is null)
        {
            throw new InvalidOperationException("Sales invoice not found.");
        }

        await _salesInvoiceRepository.DeleteSalesInvoiceAsync(invoice, cancellationToken);
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

        // Build HTML table for items
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
                    <h1 style='margin: 0; font-size: 26px; font-weight: 700; letter-spacing: 0.5px;'>VehiStock Auto Parts</h1>
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

        var subject = $"Your Invoice from VehiStock Auto Parts ({invoice.InvoiceNo})";
        
        try
        {
            await _emailService.SendInvoiceEmail(customerEmail, subject, htmlBody);
        }
        catch
        {
            // Fail silently so SMTP config issues do not block DB transactions
        }
        
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
            Items = itemsOverride ?? invoice.Items?.Select(item => new SalesInvoiceItemResponse
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
