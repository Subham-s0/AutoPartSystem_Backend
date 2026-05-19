namespace VehiStock.Application.Dtos.Auth;

public class PasswordResetResult
{
    public bool Succeeded { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();

    public static PasswordResetResult Success()
    {
        return new PasswordResetResult { Succeeded = true };
    }

    public static PasswordResetResult Failure(params string[] errors)
    {
        return new PasswordResetResult { Succeeded = false, Errors = errors };
    }

    public static PasswordResetResult Failure(IEnumerable<string> errors)
    {
        return new PasswordResetResult { Succeeded = false, Errors = errors.ToList() };
    }
}
