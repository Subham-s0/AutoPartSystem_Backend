using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

internal static class KhaltiPaymentStatusMapper
{
    public static bool ShouldApplyPayment(string khaltiStatus) =>
        string.Equals(khaltiStatus.Trim(), "Completed", StringComparison.OrdinalIgnoreCase);

    public static string MapLookupStatusToPaymentStatusString(string khaltiStatus)
    {
        var mapped = MapLookupStatusToPaymentStatus(khaltiStatus);
        return mapped?.ToString() ?? "Unpaid";
    }

    /// <summary>
    /// Maps Khalti ePayment lookup status strings to our <see cref="PaymentStatus"/> values.
    /// Completed is resolved after payment is applied to the invoice.
    /// </summary>
    public static PaymentStatus? MapLookupStatusToPaymentStatus(string khaltiStatus)
    {
        if (string.IsNullOrWhiteSpace(khaltiStatus))
        {
            return PaymentStatus.Unpaid;
        }

        return khaltiStatus.Trim().ToLowerInvariant() switch
        {
            "completed" => null,
            "refunded" => PaymentStatus.Cancelled,
            "pending" or "initiated" or "expired" or "user canceled" or "canceled" or "cancelled"
                => PaymentStatus.Unpaid,
            _ => PaymentStatus.Unpaid
        };
    }

    public static PaymentStatus ResolveInvoiceStatusAfterPayment(decimal totalAmount, decimal amountPaid)
    {
        if (totalAmount <= 0m || amountPaid >= totalAmount - 0.005m)
        {
            return PaymentStatus.Paid;
        }

        if (amountPaid <= 0m)
        {
            return PaymentStatus.Unpaid;
        }

        return PaymentStatus.Partial;
    }
}
