using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class ServiceInvoiceService : IServiceInvoiceService
{
    private readonly IServiceRecordRepository _serviceRecordRepository;
    private readonly IEmailService _emailService;

    public ServiceInvoiceService(
        IServiceRecordRepository serviceRecordRepository,
        IEmailService emailService)
    {
        _serviceRecordRepository = serviceRecordRepository;
        _emailService = emailService;
    }

    public async Task<ServiceInvoiceResponse> CreateAsync(
        int serviceRecordId,
        CancellationToken cancellationToken = default)
    {
        var serviceRecord = await _serviceRecordRepository.GetByIdAsync(serviceRecordId, cancellationToken);
        if (serviceRecord is null)
        {
            throw new InvalidOperationException("Service record not found.");
        }

        if (serviceRecord.ServiceInvoice is not null)
        {
            throw new InvalidOperationException("Service invoice already exists for this record.");
        }

        if (serviceRecord.Status != ServiceRecordStatus.ReadyForBilling)
        {
            throw new InvalidOperationException("Service record must be in ReadyForBilling status to create an invoice.");
        }

        var subtotal = serviceRecord.LaborCharge + serviceRecord.PartsCharge;
        var discountAmount = 0m;
        var taxAmount = 0m;
        var totalAmount = subtotal - discountAmount + taxAmount;

        var serviceInvoice = new ServiceInvoice
        {
            ServiceRecordId = serviceRecordId,
            CustomerId = serviceRecord.CustomerId,
            VehicleId = serviceRecord.VehicleId,
            LaborCharge = serviceRecord.LaborCharge,
            PartsCharge = serviceRecord.PartsCharge,
            DiscountPercent = 0m,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            AmountPaid = 0m,
            BalanceDue = totalAmount,
            PaymentStatus = PaymentStatus.Unpaid
        };

        serviceRecord.ServiceInvoice = serviceInvoice;
        serviceRecord.Status = ServiceRecordStatus.Closed;

        await _serviceRecordRepository.UpdateAsync(serviceRecord, cancellationToken);

        // Send Email Notification Automatically
        try
        {
            var customerEmail = serviceRecord.Customer?.User?.Email;
            if (!string.IsNullOrWhiteSpace(customerEmail))
            {
                var customerName = serviceRecord.Customer?.User?.FullName ?? "Customer";
                var invoiceNo = $"SRV-INV-{serviceInvoice.ServiceInvoiceId:D6}";

                var partsHtml = string.Join("", serviceRecord.PartsUsed.Select(item => $@"
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
                            <p style='margin: 5px 0 0 0; opacity: 0.9; font-size: 14px;'>Service Invoice #{invoiceNo}</p>
                        </div>
                        
                        <div style='padding: 30px; background-color: #ffffff;'>
                            <p style='font-size: 16px; color: #334155; margin-top: 0;'>Hello <strong>{customerName}</strong>,</p>
                            <p style='font-size: 16px; color: #334155; line-height: 1.5;'>Your vehicle servicing has been completed. Here are the invoice details for your service record:</p>
                            
                            <div style='margin: 15px 0; padding: 15px; background-color: #f8fafc; border-radius: 6px; border: 1px solid #f1f5f9; font-size: 14px; color: #475569;'>
                                <div><strong>Vehicle Plate:</strong> {serviceRecord.Vehicle?.VehicleNumber}</div>
                                <div><strong>Diagnosis:</strong> {serviceRecord.Diagnosis}</div>
                                <div><strong>Work Done:</strong> {serviceRecord.WorkDone}</div>
                            </div>

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
                                    {(serviceRecord.PartsUsed.Any() ? partsHtml : "<tr><td colspan='4' style='padding: 10px; text-align: center; color: #94a3b8;'>No parts used</td></tr>")}
                                </tbody>
                            </table>

                            <div style='text-align: right; font-size: 15px; color: #334155; border-top: 1px solid #f1f5f9; padding-top: 15px;'>
                                <p style='margin: 5px 0;'><strong>Labor Charge:</strong> NPR {serviceInvoice.LaborCharge:N2}</p>
                                <p style='margin: 5px 0;'><strong>Parts Charge:</strong> NPR {serviceInvoice.PartsCharge:N2}</p>
                                <p style='margin: 10px 0 0 0; font-size: 18px; color: #059669;'><strong>Total Amount:</strong> NPR {serviceInvoice.TotalAmount:N2}</p>
                            </div>

                            <p style='font-size: 14px; color: #64748b; text-align: center; margin-top: 35px; padding-top: 20px; border-top: 1px solid #f1f5f9; line-height: 1.5;'>
                                If you have any questions, please reply to this email or contact us at <a href='mailto:vehistock@gmail.com' style='color: #059669; text-decoration: none; font-weight: 500;'>vehistock@gmail.com</a>.
                            </p>
                        </div>
                    </div>
                ";

                var subject = $"Your Service Invoice from VehiAutoPart ({invoiceNo})";
                await _emailService.SendEmailAsync(customerEmail, subject, htmlBody);
            }
        }
        catch
        {
            // Fail silently so SMTP config issues do not block DB transactions
        }

        return new ServiceInvoiceResponse
        {
            ServiceInvoiceId = serviceInvoice.ServiceInvoiceId,
            ServiceRecordId = serviceInvoice.ServiceRecordId,
            CustomerId = serviceInvoice.CustomerId,
            VehicleId = serviceInvoice.VehicleId,
            LaborCharge = serviceInvoice.LaborCharge,
            PartsCharge = serviceInvoice.PartsCharge,
            DiscountPercent = serviceInvoice.DiscountPercent,
            TaxAmount = serviceInvoice.TaxAmount,
            TotalAmount = serviceInvoice.TotalAmount,
            AmountPaid = serviceInvoice.AmountPaid,
            BalanceDue = serviceInvoice.BalanceDue,
            PaymentStatus = serviceInvoice.PaymentStatus.ToString()
        };
    }
}
