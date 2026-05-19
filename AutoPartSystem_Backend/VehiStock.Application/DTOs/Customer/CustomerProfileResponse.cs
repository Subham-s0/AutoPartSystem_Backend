namespace VehiStock.Application.Dtos.Customer;

public class CustomerProfileResponse
{
    public int CustomerId { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? PhoneNumber { get; init; }

    public string Address { get; init; } = string.Empty;
}
