using VehiStock.Application.Dtos.Common;

namespace VehiStock.Application.Interfaces.IServices;

public interface IImageStorageService
{
    Task<string> SaveImageAsync(ImageUploadFile file, string folder, CancellationToken cancellationToken = default);

    void DeleteImage(string? imageUrl);
}
