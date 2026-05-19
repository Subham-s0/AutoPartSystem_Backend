using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class CustomerServiceInvoiceService : ICustomerServiceInvoiceService
{
    private const decimal LoyaltyThresholdAmount = 5000m;
    private const decimal LoyaltyDiscountPercent = 10m;

    private readonly ICustomerProfileRepository _customerProfileRepository;
    private readonly ICustomerServiceInvoiceRepository _customerServiceInvoiceRepository;

    public CustomerServiceInvoiceService(
        ICustomerProfileRepository customerProfileRepository,
        ICustomerServiceInvoiceRepository customerServiceInvoiceRepository)
    {
        _customerProfileRepository = customerProfileRepository;
        _customerServiceInvoiceRepository = customerServiceInvoiceRepository;
    }

    public async Task<PaginatedResponse<ServiceInvoiceListResponse>> GetServiceInvoicesPageAsync(
        string userId,
        ServiceInvoiceQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerAsync(userId, cancellationToken);
        NormalizeQuery(request);
        var invoices = await _customerServiceInvoiceRepository.GetServiceInvoicesPageAsync(customer.CustomerId, request, cancellationToken);

        return new PaginatedResponse<ServiceInvoiceListResponse>
        {
            Items = invoices.Items.Select(MapServiceInvoice).ToList(),
            PageNumber = invoices.PageNumber,
            PageSize = invoices.PageSize,
            TotalRecords = invoices.TotalRecords,
            TotalPages = invoices.TotalPages
        };
    }

    public async Task<ServiceInvoiceListResponse> GetServiceInvoiceDetailAsync(
        string userId,
        int serviceInvoiceId,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerAsync(userId, cancellationToken);
        var invoice = await _customerServiceInvoiceRepository.GetServiceInvoiceForCustomerAsync(customer.CustomerId, serviceInvoiceId, cancellationToken);

        if (invoice is null)
            throw new InvalidOperationException("Service invoice was not found.");

        return MapServiceInvoice(invoice);
    }

    public async Task<ServiceInvoiceListResponse> SetLoyaltyAsync(
        string userId,
        int serviceInvoiceId,
        SetServiceInvoiceLoyaltyRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerAsync(userId, cancellationToken);
        var invoice = await _customerServiceInvoiceRepository.GetServiceInvoiceForCustomerAsync(
            customer.CustomerId,
            serviceInvoiceId,
            cancellationToken);

        if (invoice is null)
            throw new InvalidOperationException("Service invoice was not found.");

        if (invoice.PaymentStatus == PaymentStatus.Cancelled)
            throw new InvalidOperationException("Cancelled invoice cannot be updated.");

        if (invoice.AmountPaid > 0m || invoice.PaymentStatus is PaymentStatus.Partial or PaymentStatus.Paid)
            throw new InvalidOperationException("Loyalty can only be changed before any payment is made.");

        var subtotal = invoice.LaborCharge + invoice.PartsCharge;
        var nextDiscountPercent = 0m;

        if (request.ApplyLoyalty)
        {
            if (subtotal <= LoyaltyThresholdAmount)
                throw new InvalidOperationException($"Loyalty discount applies only when service subtotal is above NPR {LoyaltyThresholdAmount:0}.");

            nextDiscountPercent = LoyaltyDiscountPercent;
        }

        invoice.DiscountPercent = nextDiscountPercent;

        var discountAmount = Math.Round(subtotal * (invoice.DiscountPercent / 100m), 2, MidpointRounding.AwayFromZero);
        invoice.TotalAmount = Math.Round(subtotal - discountAmount + invoice.TaxAmount, 2, MidpointRounding.AwayFromZero);
        invoice.BalanceDue = Math.Round(invoice.TotalAmount - invoice.AmountPaid, 2, MidpointRounding.AwayFromZero);

        if (invoice.BalanceDue <= 0.005m)
            invoice.BalanceDue = 0m;

        invoice.PaymentStatus = ResolvePaymentStatus(invoice.TotalAmount, invoice.AmountPaid);

        await _customerServiceInvoiceRepository.SaveChangesAsync(cancellationToken);

        return MapServiceInvoice(invoice);
    }

    private async Task<CustomerProfile> GetCustomerAsync(string userId, CancellationToken cancellationToken)
    {
        var customer = await _customerProfileRepository.GetCustomerProfileByUserIdAsync(userId, cancellationToken);
        if (customer is null)
            throw new InvalidOperationException("Customer profile was not found for this account.");
        return customer;
    }

    private static void NormalizeQuery(ServiceInvoiceQueryRequest request)
    {
        request.PageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        request.PageSize = Math.Clamp(request.PageSize, 1, 50);
    }

    private static ServiceInvoiceListResponse MapServiceInvoice(ServiceInvoice invoice)
    {
        return new ServiceInvoiceListResponse
        {
            ServiceInvoiceId = invoice.ServiceInvoiceId,
            ServiceRecordId = invoice.ServiceRecordId,
            ServiceDate = invoice.ServiceRecord.ServiceDate,
            VehicleNumber = invoice.Vehicle.VehicleNumber,
            Diagnosis = invoice.ServiceRecord.Diagnosis,
            ServiceStatus = invoice.ServiceRecord.Status.ToString(),
            StaffMemberName = invoice.ServiceRecord.StaffMember?.User?.FullName ?? string.Empty,
            LaborCharge = invoice.LaborCharge,
            PartsCharge = invoice.PartsCharge,
            DiscountPercent = invoice.DiscountPercent,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            AmountPaid = invoice.AmountPaid,
            BalanceDue = invoice.BalanceDue,
            PaymentStatus = invoice.PaymentStatus.ToString(),
            PartsUsed = invoice.ServiceRecord.PartsUsed
                .Select(p => new ServiceHistoryPartResponse
                {
                    PartName = p.Part.PartName,
                    Brand = p.Part.Brand,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice,
                    LineTotal = p.LineTotal
                })
                .ToList()
        };
    }

    private static PaymentStatus ResolvePaymentStatus(decimal totalAmount, decimal amountPaid)
    {
        if (totalAmount == 0m || amountPaid == totalAmount)
            return PaymentStatus.Paid;

        if (amountPaid == 0m)
            return PaymentStatus.Unpaid;

        return PaymentStatus.Partial;
    }
}
