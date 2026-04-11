using Shopfinity.Application.Features.Carts.DTOs;
using Shopfinity.Application.Features.Carts.Services;
using Shopfinity.Domain.Entities;
using Shopfinity.Tests.Helpers;
using Xunit;

namespace Shopfinity.Tests.Features.Carts;

public class CartServiceTests
{
    private const string UserId = "test-user-001";

    [Fact]
    public async Task GetCartAsync_ReturnsEmptyCartWhenNoneExists()
    {
        using var ctx = TestDbContextFactory.Create();
        var service = new CartService(ctx);

        var result = await service.GetCartAsync(UserId);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task AddToCartAsync_CreatesNewCartAndAddsItem()
    {
        using var ctx = TestDbContextFactory.Create();
        var productId = Guid.NewGuid();
        ctx.Products.Add(new Product { Id = productId, Name = "Laptop", Slug = "laptop", Price = 999m, CategoryId = Guid.NewGuid(), StockQuantity = 10 });
        await ctx.SaveChangesAsync();

        var service = new CartService(ctx);
        var result = await service.AddToCartAsync(UserId, new AddToCartDto { ProductId = productId, Quantity = 2 });

        Assert.Single(result.Items);
        Assert.Equal(productId, result.Items.First().ProductId);
        Assert.Equal(2, result.Items.First().Quantity);
    }

    [Fact]
    public async Task AddToCartAsync_ThrowsWhenQuantityIsZero()
    {
        using var ctx = TestDbContextFactory.Create();
        var service = new CartService(ctx);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AddToCartAsync(UserId, new AddToCartDto { ProductId = Guid.NewGuid(), Quantity = 0 }));
    }

    [Fact]
    public async Task AddToCartAsync_ThrowsWhenInsufficientStock()
    {
        using var ctx = TestDbContextFactory.Create();
        var productId = Guid.NewGuid();
        ctx.Products.Add(new Product { Id = productId, Name = "Laptop", Slug = "laptop", Price = 999m, CategoryId = Guid.NewGuid(), StockQuantity = 5 });
        await ctx.SaveChangesAsync();

        var service = new CartService(ctx);
        
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AddToCartAsync(UserId, new AddToCartDto { ProductId = productId, Quantity = 10 }));
    }

    [Fact]
    public async Task GetCartAsync_CleansUpPhantomProducts()
    {
        using var ctx = TestDbContextFactory.Create();
        var cart = new Cart { UserId = UserId };
        var productId = Guid.NewGuid();
        
        // Add item manually to DB without a matching product
        cart.Items.Add(new CartItem { ProductId = productId, Quantity = 1 });
        ctx.Carts.Add(cart);
        await ctx.SaveChangesAsync();

        var service = new CartService(ctx);
        var result = await service.GetCartAsync(UserId);

        Assert.Empty(result.Items);
        Assert.Empty(ctx.CartItems.ToList()); // Verified DB cleanup
    }

    [Fact]
    public async Task AddToCartAsync_ThrowsWhenProductNotFound()
    {
        using var ctx = TestDbContextFactory.Create();
        var service = new CartService(ctx);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.AddToCartAsync(UserId, new AddToCartDto { ProductId = Guid.NewGuid(), Quantity = 1 }));
    }
}
