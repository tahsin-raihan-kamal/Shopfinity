using Microsoft.EntityFrameworkCore;
using Shopfinity.Application.Common.Interfaces;
using Shopfinity.Application.Features.Orders.DTOs;
using Shopfinity.Domain.Entities;
using Shopfinity.Domain.Enums;
using System.Threading;

namespace Shopfinity.Application.Features.Orders.Services;

public interface IOrderService
{
    Task<OrderResponseDto>              CheckoutCartAsync(string userId, string? idempotencyKey = null, CancellationToken ct = default);
    Task<IEnumerable<OrderResponseDto>> GetUserOrdersAsync(string userId, CancellationToken ct = default);
    Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync(CancellationToken ct = default);
    Task<OrderResponseDto>              UpdateOrderStatusAsync(Guid orderId, OrderStatus status, CancellationToken ct = default);
}

public class OrderService : IOrderService
{
    private readonly IAppDbContext _context;

    public OrderService(IAppDbContext context) => _context = context;

    private bool UseExecuteUpdateForStock =>
        _context is DbContext d &&
        d.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";

    public async Task<OrderResponseDto> CheckoutCartAsync(string userId, string? idempotencyKey = null, CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var existingOrder = await _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.IdempotencyKey == idempotencyKey, ct);

            if (existingOrder != null) return MapOrder(existingOrder);
        }

        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (cart == null)
            throw new KeyNotFoundException("Cart was not found for this user.");

        if (!cart.Items.Any())
            throw new InvalidOperationException("Cart is empty.");

        await using var transaction = await _context.BeginTransactionAsync(
            System.Data.IsolationLevel.RepeatableRead, ct);

        try
        {
            var order = new Order
            {
                UserId           = userId,
                Status           = OrderStatus.Processing,
                IdempotencyKey   = idempotencyKey,
                TotalAmount      = 0m
            };

            foreach (var item in cart.Items.ToList())
            {
                if (item.Product == null)
                    throw new KeyNotFoundException($"Product {item.ProductId} is no longer available.");

                if (UseExecuteUpdateForStock)
                {
                    var rows = await _context.Products
                        .Where(p => p.Id == item.ProductId && p.StockQuantity >= item.Quantity)
                        .ExecuteUpdateAsync(
                            s => s.SetProperty(p => p.StockQuantity, p => p.StockQuantity - item.Quantity),
                            ct);

                    if (rows != 1)
                    {
                        throw new InvalidOperationException(
                            $"Insufficient stock for '{item.Product.Name}'. " +
                            $"Requested: {item.Quantity}, available: {item.Product.StockQuantity} (or stock changed during checkout).");
                    }
                }
                else
                {
                    if (item.Product.StockQuantity < item.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Insufficient stock for '{item.Product.Name}'. " +
                            $"Requested: {item.Quantity}, Available: {item.Product.StockQuantity}.");
                    }

                    item.Product.StockQuantity -= item.Quantity;
                }

                var lineTotal = item.Product.Price * item.Quantity;
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity  = item.Quantity,
                    UnitPrice = item.Product.Price
                });

                order.TotalAmount += lineTotal;
            }

            _context.Orders.Add(order);
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync(ct);

            if (transaction != null) await transaction.CommitAsync(ct);
            return MapOrder(order);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (transaction != null) await transaction.RollbackAsync(ct);
            throw new InvalidOperationException(
                "Stock changed or another checkout is in progress. Please review your cart.");
        }
        catch
        {
            if (transaction != null) await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IEnumerable<OrderResponseDto>> GetUserOrdersAsync(string userId, CancellationToken ct = default)
    {
        var orders = await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);

        return orders.Select(MapOrder);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync(CancellationToken ct = default)
    {
        var orders = await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);

        return orders.Select(MapOrder);
    }

    public async Task<OrderResponseDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, CancellationToken ct = default)
    {
        var order = await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        order.Status = status;
        await _context.SaveChangesAsync(ct);
        return MapOrder(order);
    }

    private static OrderResponseDto MapOrder(Order order) => new()
    {
        Id          = order.Id,
        Status      = order.Status,
        TotalAmount = order.TotalAmount,
        CreatedAt   = order.CreatedAt,
        Items       = order.Items.Select(i => new OrderItemDto
        {
            ProductId   = i.ProductId,
            ProductName = i.Product?.Name ?? "Unknown",
            Quantity    = i.Quantity,
            UnitPrice   = i.UnitPrice
        }).ToList()
    };
}
