using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VehiStock.Entities;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ProfilePhotoUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public StaffProfile? StaffProfile { get; set; }

    public CustomerProfile? CustomerProfile { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public ICollection<PurchaseInvoice> PurchaseInvoicesCreated { get; set; } = new List<PurchaseInvoice>();
}
