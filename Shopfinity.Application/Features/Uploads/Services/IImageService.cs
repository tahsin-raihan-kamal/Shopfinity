using System.IO;
using System.Threading;

namespace Shopfinity.Application.Features.Uploads.Services;

public interface IImageService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType, string basePath, CancellationToken ct = default);
}
