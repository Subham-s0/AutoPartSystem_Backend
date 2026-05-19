namespace VehiStock.Domain.Seeders;

public class AdminSeedSettings
{
    public string FullName { get; set; } = "System Administrator";
    public string Email { get; set; } = "admin@vehistock.com";
    public string Password { get; set; } = "Admin12345";
    public string? PhoneNumber { get; set; }
    public string? ProfilePhotoUrl { get; set; }
}
