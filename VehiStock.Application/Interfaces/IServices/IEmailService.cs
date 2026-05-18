namespace VehiStock.Application.Interfaces.IServices
{
    public interface IEmailService
    {
        Task SendInvoiceEmail(string toEmail, string subject, string htmlBody);
    }
}