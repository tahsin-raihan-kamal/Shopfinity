using Shopfinity.Application.Features.Uploads.Services;
using System.Threading;

namespace Shopfinity.Infrastructure.Services;

public class ImageService : IImageService
{
    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType, string basePath, CancellationToken ct = default)
    {
        if (fileStream.Length == 0)
            throw new InvalidOperationException("File is empty.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Unsupported file extension.");

        if (fileStream.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException("File exceeds the 5MB size limit.");

        var newFileName = $"{Guid.NewGuid()}{extension}";
        var folderPath = Path.Combine(basePath, "uploads");
        
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, newFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream, ct);
        }

        return $"/uploads/{newFileName}";
    }
}
