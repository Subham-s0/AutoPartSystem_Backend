namespace VehiStock.Application.Interfaces.IServices;

public class KhaltiInitiateInput
{
    public required string PurchaseOrderId { get; init; }

    public required string PurchaseOrderName { get; init; }

    public required int AmountPaisa { get; init; }

    public string? CustomerName { get; init; }

    public string? CustomerEmail { get; init; }

    public string? CustomerPhone { get; init; }
}

public class KhaltiInitiateResult
{
    public required string Pidx { get; init; }

    public required string PaymentUrl { get; init; }

    public DateTime? ExpiresAt { get; init; }
}

public class KhaltiLookupResult
{
    public required string Pidx { get; init; }

    public required string Status { get; init; }

    public string? TransactionId { get; init; }

    public int TotalAmountPaisa { get; init; }

    public int FeePaisa { get; init; }

    public bool Refunded { get; init; }
}

public interface IKhaltiClient
{
    Task<KhaltiInitiateResult> InitiateAsync(KhaltiInitiateInput input, CancellationToken cancellationToken = default);

    Task<KhaltiLookupResult> LookupAsync(string pidx, CancellationToken cancellationToken = default);
}
