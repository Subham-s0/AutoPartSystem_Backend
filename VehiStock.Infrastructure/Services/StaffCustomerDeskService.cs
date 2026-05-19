using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class StaffCustomerDeskService : IStaffCustomerDeskService
{
    private readonly IStaffCustomerDeskRepository _staffCustomerDeskRepository;
    private readonly ICustomerHistoryRepository _customerHistoryRepository;

    public StaffCustomerDeskService(
        IStaffCustomerDeskRepository staffCustomerDeskRepository,
        ICustomerHistoryRepository customerHistoryRepository)
    {
        _staffCustomerDeskRepository = staffCustomerDeskRepository;
        _customerHistoryRepository = customerHistoryRepository;
    }

    public async Task<IReadOnlyCollection<CustomerDeskDetailsResponse>> SearchAsync(
        string? fullname,
        string? customerPhone,
        string? vehicleNumber,
        int? customerId,
        string? emailId,
        CancellationToken cancellationToken = default)
    {
        var customers = await _staffCustomerDeskRepository.SearchCustomersAsync(
            fullname,
            customerPhone,
            vehicleNumber,
            customerId,
            emailId,
            cancellationToken);

        return customers.Select(MapDetails).ToList();
    }

    public async Task<CustomerDeskDetailsResponse> GetDetailsAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _staffCustomerDeskRepository.GetCustomerWithVehiclesAsync(customerId, cancellationToken);
        if (customer is null)
        {
            throw new InvalidOperationException("Customer was not found.");
        }

        return MapDetails(customer);
    }

    public async Task<IReadOnlyCollection<CustomerDeskHistoryLineResponse>> GetPurchaseHistoryAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _staffCustomerDeskRepository.GetCustomerWithVehiclesAsync(customerId, cancellationToken);
        if (customer is null)
        {
            throw new InvalidOperationException("Customer was not found.");
        }

        var invoices = await _customerHistoryRepository.GetPurchaseHistoryAsync(customerId, cancellationToken);

        return invoices
            .SelectMany(invoice => invoice.Items.Select(item => new CustomerDeskHistoryLineResponse
            {
                PartName = item.Part.PartName,
                Quantity = item.Quantity,
                TotalPrice = item.LineTotal,
                Date = invoice.InvoiceDate.ToDateTime(TimeOnly.MinValue)
            }))
            .OrderByDescending(x => x.Date)
            .ToList();
    }

    private static CustomerDeskDetailsResponse MapDetails(CustomerProfile customer)
    {
        return new CustomerDeskDetailsResponse
        {
            CustomerId = customer.CustomerId,
            Fullname = customer.User.FullName,
            Phone = customer.User.PhoneNumber ?? string.Empty,
            Email = customer.User.Email ?? string.Empty,
            Vehicles = customer.Vehicles
                .OrderBy(x => x.VehicleNumber)
                .Select(x => x.VehicleNumber)
                .ToList()
        };
    }
}
