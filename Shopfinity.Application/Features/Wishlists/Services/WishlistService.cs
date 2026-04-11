using Microsoft.EntityFrameworkCore;
using Shopfinity.Application.Common.Exceptions;
using Shopfinity.Application.Common.Interfaces;
using Shopfinity.Application.Features.Wishlists.DTOs;
using Shopfinity.Domain.Entities;
using System.Threading;

namespace Shopfinity.Application.Features.Wishlists.Services;

public class WishlistService : IWishlistService
{
    private readonly IAppDbContext _db;
    public WishlistService(IAppDbContext db) => _db = db;

    public async Task<IEnumerable<WishlistItemDto>> GetUserWishlistAsync(string userId, CancellationToken ct = default)
    {
        return await _db.WishlistItems
            .Include(x => x.Product)
            .Where(x => x.UserId == userId)
            .Select(x => new WishlistItemDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductSlug = x.Product.Slug,
                ProductName = x.Product.Name,
                ProductImageUrl = x.Product.ImageUrl,
                Price = x.Product.Price,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<WishlistItemDto> AddItemAsync(string userId, AddWishlistItemDto dto, CancellationToken ct = default)
    {
        var product = await _db.Products.FindAsync(new object[] { dto.ProductId }, ct);
        if (product == null) throw new KeyNotFoundException("Product not found.");

        var existing = await _db.WishlistItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == dto.ProductId, ct);
        if (existing != null)
            throw new ConflictException("This product is already in your wishlist.");

        var item = new WishlistItem
        {
            UserId = userId,
            ProductId = dto.ProductId
        };

        _db.WishlistItems.Add(item);
        await _db.SaveChangesAsync(ct);

        return new WishlistItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductSlug = product.Slug,
            ProductName = product.Name,
            ProductImageUrl = product.ImageUrl,
            Price = product.Price,
            CreatedAt = item.CreatedAt
        };
    }

    public async Task RemoveItemAsync(string userId, Guid id, CancellationToken ct = default)
    {
        var item = await _db.WishlistItems.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
        if (item == null) throw new KeyNotFoundException("Wishlist item not found.");

        _db.WishlistItems.Remove(item);
        await _db.SaveChangesAsync(ct);
    }
}
