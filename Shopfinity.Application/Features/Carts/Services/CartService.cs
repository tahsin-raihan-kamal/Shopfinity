using Microsoft.EntityFrameworkCore;
using Shopfinity.Application.Common.Interfaces;
using Shopfinity.Application.Features.Carts.DTOs;
using Shopfinity.Domain.Entities;
using Npgsql;
using System.Threading;

namespace Shopfinity.Application.Features.Carts.Services;

public interface ICartService
{
    Task<CartResponseDto> GetCartAsync(string userId, CancellationToken cancellationToken = default);
    Task<CartResponseDto> AddToCartAsync(string userId, AddToCartDto dto, CancellationToken cancellationToken = default);
    Task<CartResponseDto> RemoveFromCartAsync(string userId, Guid cartItemId, CancellationToken cancellationToken = default);
}

public class CartService : ICartService
{
    private readonly IAppDbContext _context;

    public CartService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CartResponseDto> GetCartAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null)
            return new CartResponseDto();

        // ── PHANTOM CLEANUP: Remove items whose products no longer exist ──────
        var orphanedItems = cart.Items.Where(i => i.Product == null).ToList();
        if (orphanedItems.Any())
        {
            foreach (var orphaned in orphanedItems)
            {
                cart.Items.Remove(orphaned);
                _context.CartItems.Remove(orphaned);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        return MapCart(cart);
    }

    public async Task<CartResponseDto> AddToCartAsync(string userId, AddToCartDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        const int maxRetries = 5;
        var currentRetry = 0;

        while (true)
        {
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

                if (cart == null)
                {
                    cart = new Cart { UserId = userId };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                var product = await _context.Products.FindAsync(new object[] { dto.ProductId }, cancellationToken);
                if (product == null) throw new KeyNotFoundException("Product not found.");

                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(i => i.CartId == cart.Id && i.ProductId == dto.ProductId, cancellationToken);

                var baseQty = existingItem?.Quantity ?? 0;
                var totalRequested = dto.Quantity + baseQty;

                if (product.StockQuantity < totalRequested)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}. " +
                        $"You already have {baseQty} in cart.");
                }

                if (existingItem != null)
                {
                    existingItem.Quantity = totalRequested;
                }
                else
                {
                    _context.CartItems.Add(new CartItem
                    {
                        CartId    = cart.Id,
                        ProductId = dto.ProductId,
                        Quantity  = dto.Quantity
                    });
                }

                await _context.SaveChangesAsync(cancellationToken);
                return await GetCartAsync(userId, cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                currentRetry++;
                if (currentRetry > maxRetries)
                    throw;

                _context.ChangeTracker.Clear();
                await Task.Delay(50 * currentRetry, cancellationToken);
            }
            catch (DbUpdateException ex) when (IsCartLineUniqueViolation(ex))
            {
                currentRetry++;
                if (currentRetry > maxRetries)
                    throw;

                _context.ChangeTracker.Clear();
                await Task.Delay(50 * currentRetry, cancellationToken);
            }
        }
    }

    private static bool IsCartLineUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException pg && pg.SqlState == "23505";

    public async Task<CartResponseDto> RemoveFromCartAsync(string userId, Guid cartItemId, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null) throw new KeyNotFoundException("Cart not found.");

        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return await GetCartAsync(userId, cancellationToken);
    }

    private CartResponseDto MapCart(Cart cart)
    {
        return new CartResponseDto
        {
            Id = cart.Id,
            Items = cart.Items.Select(i => new CartItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductSlug = i.Product?.Slug ?? string.Empty,
                ProductName = i.Product?.Name ?? "Unknown",
                ProductImageUrl = i.Product?.ImageUrl,
                Quantity = i.Quantity,
                UnitPrice = i.Product?.Price ?? 0
            }).ToList()
        };
    }
}
