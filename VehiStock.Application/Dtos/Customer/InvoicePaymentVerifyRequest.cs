using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Customer;

public class InvoicePaymentVerifyRequest
{
    [Required]
    public string Pidx { get; set; } = string.Empty;

    /// <summary>
    /// The purchase_order_id returned by Khalti on the callback URL. Required so the server
    /// can resolve the related ServiceInvoice (Khalti's lookup API does not echo this field).
    /// </summary>
    [Required]
    public string PurchaseOrderId { get; set; } = string.Empty;
}
