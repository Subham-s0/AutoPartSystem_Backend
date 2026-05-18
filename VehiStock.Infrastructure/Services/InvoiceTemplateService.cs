namespace VehiStock.Infrastructure.Services
{
    public class InvoiceTemplateService
    {
        public string Generate(string name, string invoiceNo, decimal total)
        {
            return
            "<html>" +
            "<body style='font-family:Arial'>" +
            "<h2>VehiStock Invoice</h2>" +
            "<p>Dear " + name + ",</p>" +
            "<p>Invoice No: " + invoiceNo + "</p>" +
            "<p>Total: Rs. " + total + "</p>" +
            "<br/><p>Thank you for your purchase.</p>" +
            "</body></html>";
        }
    }
}