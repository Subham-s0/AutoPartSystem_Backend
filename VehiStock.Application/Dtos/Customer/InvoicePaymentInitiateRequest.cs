using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Customer;

public class InvoicePaymentInitiateRequest
{
    [Range(0.01, 10_000_000, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }
}
