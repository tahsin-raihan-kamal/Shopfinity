using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopfinity.API.Responses;
using Shopfinity.Application.Features.Orders.DTOs;
using Shopfinity.Application.Features.Orders.Services;
using Shopfinity.Domain.Constants;
using Shopfinity.Domain.Enums;
using System.Security.Claims;
using System.Threading;

namespace Shopfinity.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _svc;
    public OrdersController(IOrderService svc) => _svc = svc;

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();

    // ── User: My Orders ───────────────────────────────────────────────────────
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponseDto>>>> GetMyOrders(CancellationToken ct)
    {
        var orders = await _svc.GetUserOrdersAsync(GetUserId(), ct);
        return Ok(ApiResponse<IEnumerable<OrderResponseDto>>.SuccessResponse(orders));
    }

    // ── User: Checkout ────────────────────────────────────────────────────────
    [HttpPost("checkout")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> Checkout(CancellationToken ct)
    {
        var rawKey = Request.Headers["X-Idempotency-Key"].FirstOrDefault();
        var idempotencyKey = string.IsNullOrWhiteSpace(rawKey) ? null : rawKey;
        var order = await _svc.CheckoutCartAsync(GetUserId(), idempotencyKey, ct);
        
        return Created(
            $"/api/v1/Orders/{order.Id}",
            ApiResponse<OrderResponseDto>.SuccessResponse(order, "Order placed successfully"));
    }

    // ── Admin: All Orders ─────────────────────────────────────────────────────
    [HttpGet("admin/all")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponseDto>>>> GetAllOrders(CancellationToken ct)
    {
        var orders = await _svc.GetAllOrdersAsync(ct);
        return Ok(ApiResponse<IEnumerable<OrderResponseDto>>.SuccessResponse(orders));
    }

    // ── Admin: Update Status ──────────────────────────────────────────────────
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> UpdateStatus(
        Guid id, [FromBody] UpdateOrderStatusDto dto, CancellationToken ct)
    {
        var order = await _svc.UpdateOrderStatusAsync(id, dto.Status, ct);
        return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(order, "Order status updated."));
    }
}
