using Shopfinity.Application.Common.Exceptions;
using Shopfinity.Application.Features.Wishlists.DTOs;
using Shopfinity.Application.Features.Wishlists.Services;
using Shopfinity.Domain.Entities;
using Shopfinity.Tests.Helpers;
using Xunit;

namespace Shopfinity.Tests.Features.Wishlists;

public class WishlistServiceTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static (WishlistService svc, Shopfinity.Infrastructure.Data.AppDbContext ctx) Build()
    {
        var ctx = TestDbContextFactory.Create();
        return (new WishlistService(ctx), ctx);
    }

    private static async Task<Product> SeedProductAsync(
        Shopfinity.Infrastructure.Data.AppDbContext ctx,
        decimal price = 99.99m)
    {
        var product = new Product
        {
            Id            = Guid.NewGuid(),
            Name          = "Sample Product",
            Slug          = $"sample-{Guid.NewGuid()}",
            Price         = price,
            StockQuantity = 20,
            CategoryId    = Guid.NewGuid()
        };
        ctx.Products.Add(product);
        await ctx.SaveChangesAsync();
        return product;
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddItemAsync_PersistsItemSuccessfully()
    {
        var (svc, ctx) = Build();
        var product    = await SeedProductAsync(ctx);

        var result = await svc.AddItemAsync("user-1", new AddWishlistItemDto { ProductId = product.Id });

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(product.Id,    result.ProductId);
        Assert.Equal(product.Name,  result.ProductName);
        Assert.Equal(product.Price, result.Price);
    }

    [Fact]
    public async Task AddItemAsync_ThrowsWhenProductNotFound()
    {
        var (svc, _) = Build();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.AddItemAsync("user-1", new AddWishlistItemDto { ProductId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task AddItemAsync_ThrowsConflictOnDuplicate()
    {
        var (svc, ctx) = Build();
        var product    = await SeedProductAsync(ctx);

        await svc.AddItemAsync("user-1", new AddWishlistItemDto { ProductId = product.Id });

        await Assert.ThrowsAsync<ConflictException>(() =>
            svc.AddItemAsync("user-1", new AddWishlistItemDto { ProductId = product.Id }));
    }

    [Fact]
    public async Task AddItemAsync_AllowsSameProductForDifferentUsers()
    {
        var (svc, ctx) = Build();
        var product    = await SeedProductAsync(ctx);

        var r1 = await svc.AddItemAsync("user-1", new AddWishlistItemDto { ProductId = product.Id });
        var r2 = await svc.AddItemAsync("user-2", new AddWishlistItemDto { ProductId = product.Id });

        Assert.NotEqual(r1.Id, r2.Id);
    }

    [Fact]
    public async Task RemoveItemAsync_RemovesCorrectItem()
    {
        var (svc, ctx) = Build();
        var product    = await SeedProductAsync(ctx);

        var item = await svc.AddItemAsync("user-1", new AddWishlistItemDto { ProductId = product.Id });
        await svc.RemoveItemAsync("user-1", item.Id);

        var remaining = await svc.GetUserWishlistAsync("user-1");
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task RemoveItemAsync_ThrowsForWrongUser()
    {
        var (svc, ctx) = Build();
        var product    = await SeedProductAsync(ctx);

        var item = await svc.AddItemAsync("user-1", new AddWishlistItemDto { ProductId = product.Id });

        // user-2 must NOT be able to remove user-1's item
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.RemoveItemAsync("user-2", item.Id));
    }

    [Fact]
    public async Task GetUserWishlistAsync_ReturnsOnlyOwnItems()
    {
        var (svc, ctx) = Build();
        var p1 = await SeedProductAsync(ctx, 10m);
        var p2 = await SeedProductAsync(ctx, 20m);

        await svc.AddItemAsync("user-1", new AddWishlistItemDto { ProductId = p1.Id });
        await svc.AddItemAsync("user-2", new AddWishlistItemDto { ProductId = p2.Id });

        var user1Items = (await svc.GetUserWishlistAsync("user-1")).ToList();

        Assert.Single(user1Items);
        Assert.Equal(p1.Id, user1Items[0].ProductId);
    }

    [Fact]
    public async Task GetUserWishlistAsync_ReturnsEmptyForNewUser()
    {
        var (svc, _) = Build();

        var items = await svc.GetUserWishlistAsync("brand-new-user");

        Assert.Empty(items);
    }
}
