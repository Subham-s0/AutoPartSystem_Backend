namespace VehiStock.Application.Dtos.Invoices
{
    public class InvoiceEmailDto
    {
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}