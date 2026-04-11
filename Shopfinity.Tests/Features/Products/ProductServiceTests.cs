using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Shopfinity.Application.Common.Mappings;
using Shopfinity.Application.Features.Products.DTOs;
using Shopfinity.Application.Features.Products.Services;
using Shopfinity.Domain.Entities;
using Shopfinity.Tests.Helpers;
using Xunit;

namespace Shopfinity.Tests.Features.Products;

public class ProductServiceTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        return config.CreateMapper();
    }

    private static IMemoryCache CreateCache() => new MemoryCache(new MemoryCacheOptions());

    [Fact]
    public async Task CreateProductAsync_PersistsProductWithSlug()
    {
        using var ctx = TestDbContextFactory.Create();
        var service = new ProductService(ctx, CreateMapper(), CreateCache());

        var dto = new CreateProductDto
        {
            Name = "Wireless Headphones",
            Description = "Premium audio",
            Price = 129.99m,
            StockQuantity = 50,
            CategoryId = Guid.NewGuid()
        };

        var result = await service.CreateProductAsync(dto);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("wireless-headphones", result.Slug);
        Assert.Equal(129.99m, result.Price);
    }

    [Fact]
    public async Task CreateProductAsync_GeneratesUniqueSlugOnConflict()
    {
        using var ctx = TestDbContextFactory.Create();
        var service = new ProductService(ctx, CreateMapper(), CreateCache());

        var dto = new CreateProductDto { Name = "Test Product", Description = "desc", Price = 10m, StockQuantity = 5, CategoryId = Guid.NewGuid() };

        var p1 = await service.CreateProductAsync(dto);
        var p2 = await service.CreateProductAsync(dto);

        Assert.NotEqual(p1.Slug, p2.Slug);
        Assert.StartsWith("test-product", p2.Slug);
    }

    [Fact]
    public async Task SearchProductsAsync_FiltersBySearchTerm()
    {
        using var ctx = TestDbContextFactory.Create();
        ctx.Products.AddRange(
            new Product { Id = Guid.NewGuid(), Name = "Blue Widget", Slug = "blue-widget", Price = 9.99m, CategoryId = Guid.NewGuid(), StockQuantity = 10 },
            new Product { Id = Guid.NewGuid(), Name = "Red Gadget", Slug = "red-gadget", Price = 19.99m, CategoryId = Guid.NewGuid(), StockQuantity = 5 }
        );
        await ctx.SaveChangesAsync();

        var service = new ProductService(ctx, CreateMapper(), CreateCache());
        var result = (await service.SearchProductsAsync(new ProductSearchDto { SearchTerm = "widget" })).Items.ToList();

        Assert.Single(result);
        Assert.Equal("Blue Widget", result[0].Name);
    }

    [Fact]
    public async Task SearchProductsAsync_FiltersByPriceRange()
    {
        using var ctx = TestDbContextFactory.Create();
        ctx.Products.AddRange(
            new Product { Id = Guid.NewGuid(), Name = "Cheap Item", Slug = "cheap", Price = 5m, CategoryId = Guid.NewGuid(), StockQuantity = 1 },
            new Product { Id = Guid.NewGuid(), Name = "Pricey Item", Slug = "pricey", Price = 500m, CategoryId = Guid.NewGuid(), StockQuantity = 1 }
        );
        await ctx.SaveChangesAsync();

        var service = new ProductService(ctx, CreateMapper(), CreateCache());
        var result = (await service.SearchProductsAsync(new ProductSearchDto { MinPrice = 10m, MaxPrice = 600m })).Items.ToList();

        Assert.Single(result);
        Assert.Equal("Pricey Item", result[0].Name);
    }

    [Fact]
    public async Task GetProductBySlugAsync_ReturnsCachedProduct()
    {
        using var ctx = TestDbContextFactory.Create();
        var slug = "cached-product";
        ctx.Products.Add(new Product { Id = Guid.NewGuid(), Name = "Cached Product", Slug = slug, Price = 99m, CategoryId = Guid.NewGuid(), StockQuantity = 10 });
        await ctx.SaveChangesAsync();

        var cache = CreateCache();
        var service = new ProductService(ctx, CreateMapper(), cache);

        // First call - fetches from DB and caches
        var result1 = await service.GetProductBySlugAsync(slug);
        // Second call - should return from cache
        var result2 = await service.GetProductBySlugAsync(slug);

        Assert.Equal(result1.Id, result2.Id);
        Assert.Equal("Cached Product", result1.Name);
    }

    [Fact]
    public async Task GetProductBySlugAsync_ThrowsForUnknownSlug()
    {
        using var ctx = TestDbContextFactory.Create();
        var service = new ProductService(ctx, CreateMapper(), CreateCache());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetProductBySlugAsync("does-not-exist"));
    }

    [Fact]
    public async Task SearchProductsAsync_SupportsPagination()
    {
        using var ctx = TestDbContextFactory.Create();
        for (int i = 1; i <= 15; i++)
        {
            ctx.Products.Add(new Product { Id = Guid.NewGuid(), Name = $"Item {i}", Slug = $"item-{i}", Price = i * 10m, CategoryId = Guid.NewGuid(), StockQuantity = i });
        }
        await ctx.SaveChangesAsync();

        var service = new ProductService(ctx, CreateMapper(), CreateCache());

        // 15 total: page 1 = 10, page 2 = 5
        var page1 = (await service.SearchProductsAsync(new ProductSearchDto { PageNumber = 1, PageSize = 10 })).Items.ToList();
        var page2 = (await service.SearchProductsAsync(new ProductSearchDto { PageNumber = 2, PageSize = 10 })).Items.ToList();

        Assert.Equal(10, page1.Count);
        Assert.Equal(5, page2.Count);
    }
}
