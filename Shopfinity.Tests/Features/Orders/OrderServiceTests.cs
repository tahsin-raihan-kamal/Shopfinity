using Microsoft.EntityFrameworkCore;
using Shopfinity.Application.Features.Carts.DTOs;
using Shopfinity.Application.Features.Carts.Services;
using Shopfinity.Application.Features.Orders.Services;
using Shopfinity.Domain.Entities;
using Shopfinity.Tests.Helpers;
using Xunit;

namespace Shopfinity.Tests.Features.Orders;

public class OrderServiceTests
{
    private const string UserId = "order-test-user";

    private static async Task<(
        CartService cartSvc,
        OrderService orderSvc,
        Shopfinity.Infrastructure.Data.AppDbContext ctx,
        Guid productId
    )> SetupAsync(int stockQuantity = 10)
    {
        var ctx = TestDbContextFactory.Create();
        var productId = Guid.NewGuid();

        ctx.Products.Add(new Product
        {
            Id = productId,
            Name = "Smart Watch",
            Slug = "smart-watch",
            Price = 249.99m,
            CategoryId = Guid.NewGuid(),
            StockQuantity = stockQuantity
        });
        await ctx.SaveChangesAsync();

        var cartSvc = new CartService(ctx);
        await cartSvc.AddToCartAsync(UserId, new AddToCartDto { ProductId = productId, Quantity = 2 });

        return (cartSvc, new OrderService(ctx), ctx, productId);
    }

    [Fact]
    public async Task CheckoutCartAsync_CreatesOrderAndClearsCart()
    {
        var (_, orderSvc, ctx, _) = await SetupAsync();

        var order = await orderSvc.CheckoutCartAsync(UserId);

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(Shopfinity.Domain.Enums.OrderStatus.Processing, order.Status);
        Assert.Single(order.Items);
        Assert.Equal(249.99m * 2, order.TotalAmount);

        // Cart must be deleted
        var carts = await ctx.Carts.Where(c => c.UserId == UserId).ToListAsync();
        Assert.Empty(carts);
    }

    [Fact]
    public async Task CheckoutCartAsync_ReturnsExistingOnIdempotencyKeyHit()
    {
        var (_, orderSvc, ctx, _) = await SetupAsync();
        const string key = "test-key-123";

        var order1 = await orderSvc.CheckoutCartAsync(UserId, key);
        var order2 = await orderSvc.CheckoutCartAsync(UserId, key);

        Assert.Equal(order1.Id, order2.Id);
        Assert.Equal(1, await ctx.Orders.CountAsync()); // Only 1 order created
    }

    [Fact]
    public async Task CheckoutCartAsync_ThrowsOnInsufficientStock()
    {
        var (_, orderSvc, ctx, productId) = await SetupAsync(stockQuantity: 10);
        var product = await ctx.Products.FindAsync(productId);
        product!.StockQuantity = 1;
        await ctx.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            orderSvc.CheckoutCartAsync(UserId));
    }

    [Fact]
    public async Task CheckoutCartAsync_DeductsStockFromProduct()
    {
        var (_, orderSvc, ctx, productId) = await SetupAsync(stockQuantity: 10);

        await orderSvc.CheckoutCartAsync(UserId);

        var product = await ctx.Products.FindAsync(productId);
        Assert.Equal(8, product!.StockQuantity); // 10 - 2 = 8
    }

    [Fact]
    public async Task CheckoutCartAsync_ThrowsWhenCartIsMissing()
    {
        using var ctx = TestDbContextFactory.Create();
        var orderSvc = new OrderService(ctx);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => orderSvc.CheckoutCartAsync("empty-cart-user"));
    }

    [Fact]
    public async Task CheckoutCartAsync_ThrowsWhenCartHasNoLineItems()
    {
        using var ctx = TestDbContextFactory.Create();
        ctx.Carts.Add(new Cart { UserId = "empty-lines-user" });
        await ctx.SaveChangesAsync();
        var orderSvc = new OrderService(ctx);

        await Assert.ThrowsAsync<InvalidOperationException>(() => orderSvc.CheckoutCartAsync("empty-lines-user"));
    }

    [Fact]
    public async Task GetUserOrdersAsync_ReturnsUserOrders()
    {
        var (_, orderSvc, ctx, _) = await SetupAsync();

        await orderSvc.CheckoutCartAsync(UserId);

        var orders = (await orderSvc.GetUserOrdersAsync(UserId)).ToList();
        Assert.Single(orders);
        Assert.Equal(249.99m * 2, orders[0].TotalAmount);
    }
}
