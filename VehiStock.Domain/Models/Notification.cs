using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiStock.Entities;

[Table("Notifications")]
public class Notification
{
    [Key]
    public int NotificationId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public NotificationType NotificationType { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
