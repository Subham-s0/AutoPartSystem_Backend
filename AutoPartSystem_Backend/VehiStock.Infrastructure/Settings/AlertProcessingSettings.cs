namespace VehiStock.Infrastructure.Settings;

public class AlertProcessingSettings
{
    public int ScanIntervalMinutes { get; set; } = 15;
    public int LowStockThreshold { get; set; } = 10;
    public int NotificationRepeatHours { get; set; } = 24;
}
