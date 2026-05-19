namespace VehiStock.Application.Dtos.Common;

public class ImageUploadFile
{
    private readonly Func<Stream> _openReadStream;
    private readonly Func<Stream, CancellationToken, Task> _copyToAsync;

    public ImageUploadFile(
        string fileName,
        string contentType,
        long length,
        Func<Stream> openReadStream,
        Func<Stream, CancellationToken, Task> copyToAsync)
    {
        FileName = fileName;
        ContentType = contentType;
        Length = length;
        _openReadStream = openReadStream;
        _copyToAsync = copyToAsync;
    }

    public string FileName { get; }

    public string ContentType { get; }

    public long Length { get; }

    public Stream OpenReadStream()
    {
        return _openReadStream();
    }

    public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
    {
        return _copyToAsync(target, cancellationToken);
    }
}
