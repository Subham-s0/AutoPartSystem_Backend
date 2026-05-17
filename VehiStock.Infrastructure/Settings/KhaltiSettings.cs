namespace VehiStock.Infrastructure.Settings;

public class KhaltiSettings
{
    public string BaseUrl { get; set; } = "https://dev.khalti.com/api/v2/";

    public string SecretKey { get; set; } = string.Empty;

    public string WebsiteUrl { get; set; } = "http://localhost:5173";

    public string ReturnUrl { get; set; } = "http://localhost:5173/customer/service-invoices/payment/callback";
}
