namespace VehiStock.Application.Interfaces.IServices
{
    public interface IEmailService
    {
        Task SendInvoiceEmail(string toEmail, string subject, string htmlBody);
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}