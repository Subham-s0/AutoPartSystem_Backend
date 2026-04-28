namespace VehiStock.Application.Dtos.Auth;

public class AuthResponse
{
    public bool Succeeded { get; init; }
    public string? UserId { get; init; }
    public string? Email { get; init; }
    public string? FullName { get; init; }
    public string? Role { get; init; }
    public int? CustomerId { get; init; }
    public int? StaffMemberId { get; init; }
    public string? AccessToken { get; init; }
    public DateTime? AccessTokenExpiresAtUtc { get; init; }
    public string? RefreshToken { get; init; }
    public DateTime? RefreshTokenExpiresAtUtc { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();

    public static AuthResponse Success(
        string userId,
        string email,
        string fullName,
        string role,
        string accessToken,
        DateTime accessTokenExpiresAtUtc,
        string refreshToken,
        DateTime refreshTokenExpiresAtUtc,
        int? customerId = null,
        int? staffMemberId = null)
    {
        return new AuthResponse
        {
            Succeeded = true,
            UserId = userId,
            Email = email,
            FullName = fullName,
            Role = role,
            CustomerId = customerId,
            StaffMemberId = staffMemberId,
            AccessToken = accessToken,
            AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc
        };
    }

    public static AuthResponse Failure(params string[] errors)
    {
        return new AuthResponse
        {
            Succeeded = false,
            Errors = errors
        };
    }

    public static AuthResponse Failure(IEnumerable<string> errors)
    {
        return new AuthResponse
        {
            Succeeded = false,
            Errors = errors.ToArray()
        };
    }
}
