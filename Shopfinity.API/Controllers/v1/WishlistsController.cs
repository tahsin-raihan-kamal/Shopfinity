using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopfinity.API.Responses;
using Shopfinity.Application.Features.Wishlists.DTOs;
using Shopfinity.Application.Features.Wishlists.Services;
using System.Security.Claims;
using System.Threading;

namespace Shopfinity.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class WishlistsController : ControllerBase
{
    private readonly IWishlistService _wishlistService;
    private readonly ILogger<WishlistsController> _logger;

    public WishlistsController(IWishlistService wishlistService, ILogger<WishlistsController> logger)
    {
        _wishlistService = wishlistService;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();

    [HttpGet]
    [ResponseCache(Duration = 30, VaryByHeader = "Cookie")] // Cache per user session briefly
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WishlistItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<WishlistItemDto>>>> GetMyWishlist(CancellationToken ct)
    {
        var items = await _wishlistService.GetUserWishlistAsync(GetUserId(), ct);
        return Ok(ApiResponse<IEnumerable<WishlistItemDto>>.SuccessResponse(items));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<WishlistItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<WishlistItemDto>>> AddToWishlist([FromBody] AddWishlistItemDto dto, CancellationToken ct)
    {
        var item = await _wishlistService.AddItemAsync(GetUserId(), dto, ct);
        _logger.LogInformation("User {UserId} added product {ProductId} to wishlist.", GetUserId(), dto.ProductId);
        return Ok(ApiResponse<WishlistItemDto>.SuccessResponse(item));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> RemoveFromWishlist(Guid id, CancellationToken ct)
    {
        await _wishlistService.RemoveItemAsync(GetUserId(), id, ct);
        _logger.LogInformation("User {UserId} removed wishlist item {ItemId}.", GetUserId(), id);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Item removed from wishlist."));
    }
}
