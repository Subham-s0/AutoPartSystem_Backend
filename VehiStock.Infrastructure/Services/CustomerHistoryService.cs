using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class CustomerHistoryService : ICustomerHistoryService
{
    private readonly ICustomerProfileRepository _customerProfileRepository;
    private readonly ICustomerHistoryRepository _customerHistoryRepository;

    public CustomerHistoryService(
        ICustomerProfileRepository customerProfileRepository,
        ICustomerHistoryRepository customerHistoryRepository)
    {
        _customerProfileRepository = customerProfileRepository;
        _customerHistoryRepository = customerHistoryRepository;
    }

    public async Task<PaginatedResponse<PurchaseHistoryResponse>> GetPurchaseHistoryPageAsync(
        string userId,
        PurchaseHistoryQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerAsync(userId, cancellationToken);
        NormalizeQuery(request);
        var purchases = await _customerHistoryRepository.GetPurchaseHistoryPageAsync(customer.CustomerId, request, cancellationToken);

        return new PaginatedResponse<PurchaseHistoryResponse>
        {
            Items = purchases.Items.Select(MapPurchaseHistory).ToList(),
            PageNumber = purchases.PageNumber,
            PageSize = purchases.PageSize,
            TotalRecords = purchases.TotalRecords,
            TotalPages = purchases.TotalPages
        };
    }

    public async Task<PaginatedResponse<ServiceHistoryResponse>> GetServiceHistoryPageAsync(
        string userId,
        ServiceHistoryQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerAsync(userId, cancellationToken);
        NormalizeQuery(request);
        var services = await _customerHistoryRepository.GetServiceHistoryPageAsync(customer.CustomerId, request, cancellationToken);

        return new PaginatedResponse<ServiceHistoryResponse>
        {
            Items = services.Items.Select(MapServiceHistory).ToList(),
            PageNumber = services.PageNumber,
            PageSize = services.PageSize,
            TotalRecords = services.TotalRecords,
            TotalPages = services.TotalPages
        };
    }

    public async Task<ServiceHistoryResponse> GetServiceHistoryDetailAsync(
        string userId,
        int serviceRecordId,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerAsync(userId, cancellationToken);
        var serviceRecord = await _customerHistoryRepository.GetServiceRecordDetailAsync(customer.CustomerId, serviceRecordId, cancellationToken);

        if (serviceRecord is null)
            throw new InvalidOperationException("Service record was not found for this customer.");

        return MapServiceHistory(serviceRecord);
    }

    private async Task<CustomerProfile> GetCustomerAsync(string userId, CancellationToken cancellationToken)
    {
        var customer = await _customerProfileRepository.GetCustomerProfileByUserIdAsync(userId, cancellationToken);
        if (customer is null)
            throw new InvalidOperationException("Customer profile was not found for this account.");
        return customer;
    }

    private static void NormalizeQuery(PurchaseHistoryQueryRequest request)
    {
        request.PageNumber = NormalizePageNumber(request.PageNumber);
        request.PageSize = NormalizePageSize(request.PageSize);
    }

    private static void NormalizeQuery(ServiceHistoryQueryRequest request)
    {
        request.PageNumber = NormalizePageNumber(request.PageNumber);
        request.PageSize = NormalizePageSize(request.PageSize);
    }

    private static int NormalizePageNumber(int pageNumber) => pageNumber < 1 ? 1 : pageNumber;
    private static int NormalizePageSize(int pageSize) => Math.Clamp(pageSize, 1, 50);

    private static PurchaseHistoryResponse MapPurchaseHistory(SalesInvoice salesInvoice)
    {
        return new PurchaseHistoryResponse
        {
            SalesInvoiceId = salesInvoice.SalesInvoiceId,
            InvoiceNo = salesInvoice.InvoiceNo,
            InvoiceDate = salesInvoice.InvoiceDate,
            VehicleNumber = salesInvoice.Vehicle.VehicleNumber,
            TotalAmount = salesInvoice.TotalAmount,
            AmountPaid = salesInvoice.AmountPaid,
            BalanceDue = salesInvoice.BalanceDue,
            PaymentStatus = salesInvoice.PaymentStatus.ToString(),
            Items = salesInvoice.Items.Select(item => new PurchaseHistoryItemResponse
            {
                PartName = item.Part.PartName,
                Brand = item.Part.Brand,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                LineTotal = item.LineTotal
            }).ToList()
        };
    }

    private static ServiceHistoryResponse MapServiceHistory(ServiceRecord serviceRecord)
    {
        return new ServiceHistoryResponse
        {
            ServiceRecordId = serviceRecord.ServiceRecordId,
            ServiceDate = serviceRecord.ServiceDate,
            ServiceStatus = serviceRecord.Status.ToString(),
            VehicleNumber = serviceRecord.Vehicle.VehicleNumber,
            Diagnosis = serviceRecord.Diagnosis,
            WorkDone = serviceRecord.WorkDone,
            LaborCharge = serviceRecord.LaborCharge,
            PartsCharge = serviceRecord.PartsCharge,
            TotalCharge = serviceRecord.TotalCharge,
            Notes = serviceRecord.Notes,
            StaffMemberName = serviceRecord.StaffMember?.User?.FullName ?? string.Empty,
            StaffJobTitle = serviceRecord.StaffMember?.JobTitle ?? string.Empty,
            ServiceInvoice = serviceRecord.ServiceInvoice is null
                ? null
                : new ServiceInvoiceSummaryResponse
                {
                    ServiceInvoiceId = serviceRecord.ServiceInvoice.ServiceInvoiceId,
                    LaborCharge = serviceRecord.ServiceInvoice.LaborCharge,
                    PartsCharge = serviceRecord.ServiceInvoice.PartsCharge,
                    DiscountPercent = serviceRecord.ServiceInvoice.DiscountPercent,
                    TaxAmount = serviceRecord.ServiceInvoice.TaxAmount,
                    TotalAmount = serviceRecord.ServiceInvoice.TotalAmount,
                    AmountPaid = serviceRecord.ServiceInvoice.AmountPaid,
                    BalanceDue = serviceRecord.ServiceInvoice.BalanceDue,
                    PaymentStatus = serviceRecord.ServiceInvoice.PaymentStatus.ToString()
                },
            PartsUsed = serviceRecord.PartsUsed.Select(part => new ServiceHistoryPartResponse
            {
                PartName = part.Part.PartName,
                Brand = part.Part.Brand,
                Quantity = part.Quantity,
                UnitPrice = part.UnitPrice,
                LineTotal = part.LineTotal
            }).ToList(),
            Review = serviceRecord.Reviews
                .OrderByDescending(review => review.CreatedAt)
                .Select(review => new ReviewResponse
                {
                    ReviewId = review.ReviewId,
                    ServiceRecordId = review.ServiceRecordId,
                    VehicleNumber = serviceRecord.Vehicle.VehicleNumber,
                    ServiceDate = serviceRecord.ServiceDate,
                    Diagnosis = serviceRecord.Diagnosis,
                    WorkDone = serviceRecord.WorkDone,
                    Rating = review.Rating,
                    ReviewText = review.ReviewText,
                    CreatedAt = review.CreatedAt
                })
                .FirstOrDefault()
        };
    }
}
