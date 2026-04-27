using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.DTOs.SalesInvoices;

public class CreateSalesInvoiceItemRequest
{
    [Required]
    public int PartId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; }
}
