using System.IO;
using Shopfinity.Infrastructure.Services;
using Xunit;

namespace Shopfinity.Tests.Features.Uploads;

public class ImageServiceTests
{
    private readonly string _tempBasePath;

    public ImageServiceTests()
    {
        _tempBasePath = Path.Combine(Path.GetTempPath(), $"shopfinity-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempBasePath);
    }

    [Fact]
    public async Task UploadImageAsync_SavesFileAndReturnsUrl()
    {
        var service = new ImageService();
        var content = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
        using var stream = new MemoryStream(content);

        var url = await service.UploadImageAsync(stream, "test.png", "image/png", _tempBasePath);

        Assert.StartsWith("/uploads/", url);
        Assert.EndsWith(".png", url);

        var savedFile = Path.Combine(_tempBasePath, "uploads", url.Replace("/uploads/", ""));
        Assert.True(File.Exists(savedFile));
    }

    [Fact]
    public async Task UploadImageAsync_ThrowsForUnsupportedExtension()
    {
        var service = new ImageService();
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadImageAsync(stream, "evil.exe", "application/octet-stream", _tempBasePath));
    }

    [Fact]
    public async Task UploadImageAsync_ThrowsForEmptyFile()
    {
        var service = new ImageService();
        using var stream = new MemoryStream(Array.Empty<byte>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadImageAsync(stream, "empty.jpg", "image/jpeg", _tempBasePath));
    }
}
