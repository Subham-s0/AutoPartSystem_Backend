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
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress(_settings.SenderEmail);
                mail.To.Add(toEmail.Trim());
                mail.Subject = subject;
                mail.Body = htmlBody;
                mail.IsBodyHtml = true;

                using (var smtp = new SmtpClient(_settings.SmtpServer, _settings.Port))
                {
                    smtp.Credentials = new NetworkCredential(
                        _settings.SenderEmail,
                        _settings.SenderPassword
                    );

                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
               
                throw new Exception("SMTP FAILED: " + ex.Message);
            }
        }
    }
}