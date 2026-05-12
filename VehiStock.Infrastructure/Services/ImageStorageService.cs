using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Infrastructure.Settings;

namespace VehiStock.Infrastructure.Services;

public class ImageStorageService : IImageStorageService
{
    private readonly IHostEnvironment _environment;
    private readonly ImageUploadSettings _settings;

    public ImageStorageService(IHostEnvironment environment, IOptions<ImageUploadSettings> options)
    {
        _environment = environment;
        _settings = options.Value;
    }

    public async Task<string> SaveImageAsync(ImageUploadFile file, string folder, CancellationToken cancellationToken = default)
    {
        ValidateImage(file);

        var uploadRoot = NormalizePathSegment(_settings.UploadRoot, "uploads");
        var targetFolder = NormalizePathSegment(folder, "general");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var webRootPath = GetWebRootPath();
        var directoryPath = Path.Combine(webRootPath, uploadRoot, targetFolder);

        Directory.CreateDirectory(directoryPath);

        var filePath = Path.Combine(directoryPath, fileName);
        await using var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/{uploadRoot}/{targetFolder}/{fileName}".Replace("\\", "/");
    }

    public void DeleteImage(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) ||
            Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
        {
            return;
        }

        var uploadRoot = NormalizePathSegment(_settings.UploadRoot, "uploads");
        var relativePath = imageUrl.Trim().TrimStart('~', '/', '\\').Replace('/', Path.DirectorySeparatorChar);

        if (!relativePath.StartsWith(uploadRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var webRootPath = GetWebRootPath();
        var uploadRootPath = Path.GetFullPath(Path.Combine(webRootPath, uploadRoot));
        var filePath = Path.GetFullPath(Path.Combine(webRootPath, relativePath));

        if (!filePath.StartsWith(uploadRootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    private void ValidateImage(ImageUploadFile file)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("Image file is empty.");
        }

        if (file.Length > _settings.MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"Image file must be {_settings.MaxFileSizeBytes / 1024 / 1024} MB or smaller.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) ||
            !_settings.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only JPG, PNG, and WEBP images are allowed.");
        }

        if (string.IsNullOrWhiteSpace(file.ContentType) ||
            !_settings.AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Uploaded file must be a valid image.");
        }

    }

    private string GetWebRootPath()
    {
        return Path.Combine(_environment.ContentRootPath, "wwwroot");
    }

    private static string NormalizePathSegment(string? value, string fallback)
    {
        var segment = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        segment = segment.Trim('/', '\\');

        if (segment.Contains("..", StringComparison.Ordinal) ||
            segment.Contains(Path.DirectorySeparatorChar) ||
            segment.Contains(Path.AltDirectorySeparatorChar))
        {
            throw new InvalidOperationException("Upload path settings are invalid.");
        }

        return segment;
    }
}
