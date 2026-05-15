using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class CustomerServiceInvoiceService : ICustomerServiceInvoiceService
{
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
            PaymentStatus = invoice.PaymentStatus.ToString()
        };
    }
}
