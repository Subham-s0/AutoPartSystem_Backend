namespace VehiStock.Application.DTOs.Staff;

public class IdentityOperationResult
{
    public bool Succeeded { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
}
