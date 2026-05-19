namespace VehiStock.Application.Dtos.Auth;

public class RegisterUserResponse
{
    public bool Succeeded { get; init; }
    public string? UserId { get; init; }
    public string? Role { get; init; }
    public int? CustomerId { get; init; }
    public int? StaffMemberId { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();

    public static RegisterUserResponse Success(
        string userId,
        string role,
        int? customerId = null,
        int? staffMemberId = null)
    {
        return new RegisterUserResponse
        {
            Succeeded = true,
            UserId = userId,
            Role = role,
            CustomerId = customerId,
            StaffMemberId = staffMemberId
        };
    }

    public static RegisterUserResponse Failure(params string[] errors)
    {
        return new RegisterUserResponse
        {
            Succeeded = false,
            Errors = errors
        };
    }

    public static RegisterUserResponse Failure(IEnumerable<string> errors)
    {
        return new RegisterUserResponse
        {
            Succeeded = false,
            Errors = errors.ToList()
        };
    }
}
