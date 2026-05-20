using System.ComponentModel.DataAnnotations;
using VehiStock.Entities;

namespace VehiStock.Application.Dtos.Staff;

// Used for sales invoice creation request
public class CreateSalesInvoiceRequest
{
    public int CustomerId { get; set; }


    public int VehicleId { get; set; }

    public DateOnly InvoiceDate { get; set; }

    [Range(0, 100)]
    public decimal DiscountPercent { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TaxAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal AmountPaid { get; set; }

    public DateOnly? CreditDueDate { get; set; }

    public PaymentType PaymentType { get; set; }

    [MinLength(1)]
    public List<CreateSalesInvoiceItemRequest> Items { get; set; } = [];
}
