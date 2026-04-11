using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopfinity.API.Responses;
using Shopfinity.Application.Features.Carts.DTOs;
using Shopfinity.Application.Features.Carts.Services;
using System.Security.Claims;
using System.Threading;

namespace Shopfinity.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CartsController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartsController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CartResponseDto>>> GetMyCart(CancellationToken ct)
    {
        var cart = await _cartService.GetCartAsync(GetUserId(), ct);
        return Ok(ApiResponse<CartResponseDto>.SuccessResponse(cart));
    }

    [HttpPost("items")]
    public async Task<ActionResult<ApiResponse<CartResponseDto>>> AddItem([FromBody] AddToCartDto dto, CancellationToken ct)
    {
        var cart = await _cartService.AddToCartAsync(GetUserId(), dto, ct);
        return Ok(ApiResponse<CartResponseDto>.SuccessResponse(cart));
    }

    [HttpDelete("items/{itemId:guid}")]
    public async Task<ActionResult<ApiResponse<CartResponseDto>>> RemoveItem(Guid itemId, CancellationToken ct)
    {
        var cart = await _cartService.RemoveFromCartAsync(GetUserId(), itemId, ct);
        return Ok(ApiResponse<CartResponseDto>.SuccessResponse(cart));
    }
}
