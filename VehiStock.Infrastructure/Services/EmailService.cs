using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Infrastructure.Configurations;

namespace VehiStock.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendInvoiceEmail(string toEmail, string subject, string htmlBody)
        {
            var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
            {
                Credentials = new NetworkCredential(
                    _settings.SenderEmail,
                    _settings.SenderPassword
                ),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
        }
    }
}