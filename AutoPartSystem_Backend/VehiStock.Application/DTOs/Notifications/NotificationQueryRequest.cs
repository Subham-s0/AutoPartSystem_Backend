namespace VehiStock.Application.Dtos.Notifications;

public class NotificationQueryRequest
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? NotificationType { get; set; }

    public bool? IsRead { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }
}
