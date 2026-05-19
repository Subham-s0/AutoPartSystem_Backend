using System;

namespace VehiStock.Infrastructure.Services
{
    public class InvoiceTemplateService
    {
        public string Generate(string name, string invoiceNo, decimal total)
        {
            var dateString = DateTime.Now.ToString("dddd, MMMM dd, yyyy h:mm tt");
            
            return $@"
<div style=""background-color: #f8fafc; padding: 40px 20px; font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 15px rgba(0, 0, 0, 0.05); border: 1px solid #e2e8f0;"">
        <!-- Top Dark Green Header Banner -->
        <div style=""background-color: #14532d; padding: 30px; text-align: center;"">
            <span style=""color: #ffffff; font-size: 32px; font-weight: 700; letter-spacing: 1px; line-height: 1;"">VehiStock</span>
        </div>
        
        <!-- Main Content Area -->
        <div style=""padding: 40px 30px;"">
            <!-- Order Confirmation Sub-Header Banner -->
            <div style=""background-color: #15803d; border-radius: 8px; padding: 15px; text-align: center; margin-bottom: 30px;"">
                <span style=""color: #ffffff; font-size: 20px; font-weight: 600; letter-spacing: 0.5px; line-height: 1;"">Order Confirmation</span>
            </div>
            
            <p style=""font-size: 16px; color: #334155; line-height: 1.6; margin: 0 0 10px 0;"">Hello <strong style=""color: #0f172a;"">{name}</strong>,</p>
            <p style=""font-size: 16px; color: #475569; line-height: 1.6; margin: 0 0 30px 0;"">Thank you for your purchase from VehiStock. We have successfully processed your transaction.</p>
            
            <!-- Details Box -->
            <div style=""background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 25px; margin-bottom: 30px;"">
                <table style=""width: 100%; border-collapse: collapse;"">
                    <tr style=""height: 35px;"">
                        <td style=""width: 40%; font-size: 13px; font-weight: 600; color: #94a3b8; text-transform: uppercase; letter-spacing: 0.5px; vertical-align: middle;"">Invoice Number</td>
                        <td style=""font-size: 15px; font-weight: 600; color: #1e293b; vertical-align: middle;"">{invoiceNo}</td>
                    </tr>
                    <tr style=""height: 35px;"">
                        <td style=""font-size: 13px; font-weight: 600; color: #94a3b8; text-transform: uppercase; letter-spacing: 0.5px; vertical-align: middle;"">Total Amount</td>
                        <td style=""font-size: 18px; font-weight: 700; color: #15803d; vertical-align: middle;"">NPR {total:N2}</td>
                    </tr>
                    <tr style=""height: 35px;"">
                        <td style=""font-size: 13px; font-weight: 600; color: #94a3b8; text-transform: uppercase; letter-spacing: 0.5px; vertical-align: middle;"">Date</td>
                        <td style=""font-size: 14px; font-weight: 500; color: #475569; vertical-align: middle;"">{dateString}</td>
                    </tr>
                </table>
            </div>
            
            <!-- Footer Text -->
            <p style=""font-size: 14px; color: #64748b; text-align: center; line-height: 1.5; margin: 0; padding-top: 20px; border-top: 1px solid #f1f5f9;"">
                You can view your full purchase history by logging into the customer portal.
            </p>
        </div>
    </div>
</div>";
        }
    }
}