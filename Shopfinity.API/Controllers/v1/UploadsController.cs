using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shopfinity.API.Responses;
using Shopfinity.Application.Features.Uploads.Services;
using Shopfinity.Domain.Constants;
using System.IO;
using System.Threading;

namespace Shopfinity.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = AppRoles.Admin)]
public class UploadsController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly IWebHostEnvironment _env;

    public UploadsController(IImageService imageService, IWebHostEnvironment env)
    {
        _imageService = imageService;
        _env = env;
    }

    [HttpPost("image")]
    public async Task<ActionResult<ApiResponse<string>>> UploadImage(IFormFile? file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("No image file was uploaded.");

        using var stream = file.OpenReadStream();
        var basePath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var url = await _imageService.UploadImageAsync(stream, file.FileName, file.ContentType, basePath, ct);
        return Ok(ApiResponse<string>.SuccessResponse(url));
    }
}
