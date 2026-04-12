using Shopfinity.Application.Features.Reviews.DTOs;
using Shopfinity.Application.Features.Reviews.Services;
using Shopfinity.Domain.Entities;
using Shopfinity.Tests.Helpers;
using Xunit;

namespace Shopfinity.Tests.Features.Reviews;

public class ReviewServiceTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static (ReviewService svc, Shopfinity.Infrastructure.Data.AppDbContext ctx) Build()
    {
        var ctx = TestDbContextFactory.Create();
        return (new ReviewService(ctx), ctx);
    }

    private static async Task<Guid> SeedProductAsync(Shopfinity.Infrastructure.Data.AppDbContext ctx)
    {
        var product = new Product
        {
            Id            = Guid.NewGuid(),
            Name          = "Test Product",
            Slug          = $"test-product-{Guid.NewGuid()}",
            Price         = 49.99m,
            StockQuantity = 10,
            CategoryId    = Guid.NewGuid()
        };
        ctx.Products.Add(product);
        await ctx.SaveChangesAsync();
        return product.Id;
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddReviewAsync_PersistsReviewSuccessfully()
    {
        var (svc, ctx) = Build();
        var productId  = await SeedProductAsync(ctx);

        var dto = new CreateReviewDto
        {
            ProductId = productId,
            Rating    = 5,
            Title     = "Great product!",
            Comment   = "Really enjoyed it."
        };

        var result = await svc.AddReviewAsync("user-1", "Alice", dto);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(5,              result.Rating);
        Assert.Equal("Great product!", result.Title);
        Assert.Equal("Alice",          result.UserName);
        Assert.Equal(productId,        result.ProductId);
    }

    [Fact]
    public async Task AddReviewAsync_ThrowsWhenProductNotFound()
    {
        var (svc, _) = Build();

        var dto = new CreateReviewDto
        {
            ProductId = Guid.NewGuid(), // non-existent
            Rating    = 4,
            Title     = "Oops",
            Comment   = "No product here."
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.AddReviewAsync("user-1", "Alice", dto));
    }

    [Fact]
    public async Task AddReviewAsync_ThrowsOnDuplicateReview()
    {
        var (svc, ctx) = Build();
        var productId  = await SeedProductAsync(ctx);

        var dto = new CreateReviewDto
        {
            ProductId = productId,
            Rating    = 3,
            Title     = "First review",
            Comment   = "OK product."
        };

        await svc.AddReviewAsync("user-1", "Alice", dto); // first — succeeds

        // Second review by the same user for the same product must throw
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.AddReviewAsync("user-1", "Alice", dto));
    }

    [Fact]
    public async Task AddReviewAsync_AllowsDifferentUsersToReviewSameProduct()
    {
        var (svc, ctx) = Build();
        var productId  = await SeedProductAsync(ctx);

        var dto = new CreateReviewDto { ProductId = productId, Rating = 4, Title = "Good", Comment = "Solid." };

        await svc.AddReviewAsync("user-1", "Alice", dto);
        var result2 = await svc.AddReviewAsync("user-2", "Bob",   dto);

        Assert.Equal("Bob", result2.UserName);
    }

    [Fact]
    public async Task GetProductReviewsAsync_ReturnsReviewsOrderedByDateDesc()
    {
        var (svc, ctx) = Build();
        var productId  = await SeedProductAsync(ctx);

        // Seed two reviews with explicit timestamps
        ctx.ProductReviews.Add(new ProductReview
        {
            Id        = Guid.NewGuid(),
            UserId    = "user-a",
            UserName  = "Alpha",
            ProductId = productId,
            Rating    = 5,
            Title     = "Older",
            Comment   = "Old",
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        });
        ctx.ProductReviews.Add(new ProductReview
        {
            Id        = Guid.NewGuid(),
            UserId    = "user-b",
            UserName  = "Beta",
            ProductId = productId,
            Rating    = 3,
            Title     = "Newer",
            Comment   = "New",
            CreatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var reviews = (await svc.GetProductReviewsAsync(productId)).ToList();

        Assert.Equal(2,       reviews.Count);
        Assert.Equal("Newer", reviews[0].Title); // most recent first
        Assert.Equal("Older", reviews[1].Title);
    }

    [Fact]
    public async Task GetProductReviewsAsync_ReturnsEmptyListForNoReviews()
    {
        var (svc, ctx) = Build();
        var productId  = await SeedProductAsync(ctx);

        var reviews = await svc.GetProductReviewsAsync(productId);

        Assert.Empty(reviews);
    }
}
