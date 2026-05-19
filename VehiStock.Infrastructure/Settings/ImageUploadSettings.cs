namespace VehiStock.Infrastructure.Settings;

public class ImageUploadSettings
{
    public string UploadRoot { get; set; } = "uploads";

    public long MaxFileSizeBytes { get; set; } = 2 * 1024 * 1024;

    public string[] AllowedContentTypes { get; set; } = [];

    public string[] AllowedExtensions { get; set; } = [];
}
