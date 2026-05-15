using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public interface IServiceInvoiceService
{
    Task<ServiceInvoiceResponse> CreateAsync(int serviceRecordId, decimal discountPercent = 0m, CancellationToken cancellationToken = default);
}

public class ServiceInvoiceService : IServiceInvoiceService
{
    private readonly IServiceRecordRepository _serviceRecordRepository;

    public ServiceInvoiceService(IServiceRecordRepository serviceRecordRepository)
    {
        _serviceRecordRepository = serviceRecordRepository;
    }

    public async Task<ServiceInvoiceResponse> CreateAsync(
        int serviceRecordId,
        decimal discountPercent = 0m,
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
        var normalizedDiscountPercent = Math.Clamp(discountPercent, 0m, 100m);
        var discountAmount = Math.Round(subtotal * (normalizedDiscountPercent / 100m), 2, MidpointRounding.AwayFromZero);
        var taxAmount = 0m;
        var totalAmount = subtotal - discountAmount + taxAmount;

        var serviceInvoice = new ServiceInvoice
        {
            ServiceRecordId = serviceRecordId,
            CustomerId = serviceRecord.CustomerId,
            VehicleId = serviceRecord.VehicleId,
            LaborCharge = serviceRecord.LaborCharge,
            PartsCharge = serviceRecord.PartsCharge,
            DiscountPercent = normalizedDiscountPercent,
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
