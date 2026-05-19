using System;
using System.Collections.Generic;
using VehiStock.Application.Dtos.Customer;

namespace VehiStock.Application.Dtos.Staff;

public class StaffCustomerResponse
{
    public int CustomerId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string Address { get; init; } = string.Empty;
    public DateTime RegisteredAt { get; init; }
    public IReadOnlyCollection<CustomerVehicleResponse> Vehicles { get; init; } = Array.Empty<CustomerVehicleResponse>();
}
