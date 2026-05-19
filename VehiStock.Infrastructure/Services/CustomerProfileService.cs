using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class CustomerProfileService : ICustomerProfileService
{
    private readonly ICustomerProfileRepository _customerProfileRepository;
    private readonly ICustomerHistoryRepository _customerHistoryRepository;

    public CustomerProfileService(
        ICustomerProfileRepository customerProfileRepository,
        ICustomerHistoryRepository customerHistoryRepository)
    {
        _customerProfileRepository = customerProfileRepository;
        _customerHistoryRepository = customerHistoryRepository;
    }

    public async Task<CustomerProfileResponse> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerAsync(userId, cancellationToken);
        return MapCustomerProfile(customer);
    }

    public async Task<PaginatedResponse<StaffCustomerResponse>> GetCustomersForStaffAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var paginated = await _customerProfileRepository.GetCustomersForStaffAsync(search, page, pageSize, cancellationToken);
        var mappedItems = paginated.Items.Select(MapStaffCustomerResponse).ToList();
        return new PaginatedResponse<StaffCustomerResponse>
        {
            Items = mappedItems,
            TotalRecords = paginated.TotalRecords,
            PageNumber = paginated.PageNumber,
            PageSize = paginated.PageSize,
            TotalPages = paginated.TotalPages
        };
    }

    public async Task<StaffCustomerHistoryResponse> GetCustomerHistoryAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _customerProfileRepository.GetCustomerProfileByIdAsync(customerId, cancellationToken);
        if (customer is null)
            throw new InvalidOperationException("Customer profile not found.");

        var purchaseHistory = await _customerHistoryRepository.GetPurchaseHistoryAsync(customerId, cancellationToken);
        // Assuming we need Service History too, wait, ICustomerHistoryRepository has GetServiceHistoryPageAsync but not a full list. Let's just use it with large page size or something.
        // Actually, ICustomerHistoryRepository.GetServiceHistoryPageAsync(customerId, new ServiceHistoryQueryRequest { PageSize = 100 }, cancellationToken)
        var serviceHistory = await _customerHistoryRepository.GetServiceHistoryPageAsync(customerId, new ServiceHistoryQueryRequest { PageNumber = 1, PageSize = 100 }, cancellationToken);

        var historyItems = new List<StaffCustomerHistoryItem>();

        foreach (var inv in purchaseHistory)
        {
            historyItems.Add(new StaffCustomerHistoryItem
            {
                Type = "SalesInvoice",
                Id = inv.SalesInvoiceId,
                Date = inv.InvoiceDate.ToDateTime(TimeOnly.MinValue),
                Description = $"Part Sales Invoice - {inv.PaymentStatus}",
                Amount = inv.TotalAmount,
                Status = inv.PaymentStatus.ToString()
            });
        }

        foreach (var srv in serviceHistory.Items)
        {
            historyItems.Add(new StaffCustomerHistoryItem
            {
                Type = "ServiceRecord",
                Id = srv.ServiceRecordId,
                Date = srv.ServiceDate.ToDateTime(TimeOnly.MinValue),
                Description = $"Service: {srv.Vehicle?.VehicleNumber} - {srv.Diagnosis}",
                Amount = srv.TotalCharge,
                Status = srv.Status.ToString()
            });
        }

        historyItems = historyItems.OrderByDescending(x => x.Date).ToList();

        return new StaffCustomerHistoryResponse
        {
            CustomerId = customer.CustomerId,
            FullName = customer.User?.FullName ?? string.Empty,
            Email = customer.User?.Email ?? string.Empty,
            PhoneNumber = customer.User?.PhoneNumber ?? string.Empty,
            TotalSpent = purchaseHistory.Where(x => x.PaymentStatus == VehiStock.Entities.PaymentStatus.Paid).Sum(x => x.TotalAmount) + 
                         serviceHistory.Items.Where(x => x.Status == VehiStock.Entities.ServiceRecordStatus.Closed).Sum(x => x.TotalCharge),
            HistoryItems = historyItems
        };
    }

    private async Task<CustomerProfile> GetCustomerAsync(string userId, CancellationToken cancellationToken)
    {
        var customer = await _customerProfileRepository.GetCustomerProfileByUserIdAsync(userId, cancellationToken);
        if (customer is null)
            throw new InvalidOperationException("Customer profile was not found for this account.");
        return customer;
    }

    private static CustomerProfileResponse MapCustomerProfile(CustomerProfile customer)
    {
        return new CustomerProfileResponse
        {
            CustomerId = customer.CustomerId,
            FullName = customer.User?.FullName ?? string.Empty,
            Email = customer.User?.Email ?? string.Empty,
            PhoneNumber = customer.User?.PhoneNumber,
            Address = customer.Address
        };
    }

    private static StaffCustomerResponse MapStaffCustomerResponse(CustomerProfile customer)
    {
        return new StaffCustomerResponse
        {
            CustomerId = customer.CustomerId,
            UserId = customer.UserId,
            FullName = customer.User?.FullName ?? string.Empty,
            Email = customer.User?.Email ?? string.Empty,
            PhoneNumber = customer.User?.PhoneNumber,
            Address = customer.Address,
            RegisteredAt = customer.CreatedAt,
            Vehicles = customer.Vehicles.Select(v => new CustomerVehicleResponse
            {
                VehicleId = v.VehicleId,
                VehicleNumber = v.VehicleNumber,
                Make = v.Make,
                Model = v.Model,
                ManufactureYear = v.ManufactureYear,
                EngineNo = v.EngineNo,
                ChassisNo = v.ChassisNo,
                VehiclePhotoUrl = v.VehiclePhotoUrl,
                MileageKm = v.MileageKm,
                Notes = v.Notes
            }).ToList()
        };
    }
}
