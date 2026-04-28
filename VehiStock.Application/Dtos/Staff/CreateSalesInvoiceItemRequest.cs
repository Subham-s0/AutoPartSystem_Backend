using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.Dtos.Staff;

// Used for sales invoice item request
public class CreateSalesInvoiceItemRequest
{
    [Range(1, int.MaxValue)]
    public int PartId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; }
}
