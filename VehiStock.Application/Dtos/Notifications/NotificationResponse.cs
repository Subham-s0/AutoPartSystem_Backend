namespace VehiStock.Application.Dtos.Notifications;

public class NotificationResponse
{
    public int NotificationId { get; init; }
    public string NotificationType { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ReferenceType { get; init; }
    public int? ReferenceId { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReadAt { get; init; }
}
