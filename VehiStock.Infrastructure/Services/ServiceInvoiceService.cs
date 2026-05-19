using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class ServiceInvoiceService : IServiceInvoiceService
{
    private readonly IServiceRecordRepository _serviceRecordRepository;

    public ServiceInvoiceService(IServiceRecordRepository serviceRecordRepository)
    {
        _serviceRecordRepository = serviceRecordRepository;
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
