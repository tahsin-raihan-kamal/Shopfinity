using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Shopfinity.Application.Common.Mappings;
using Shopfinity.Application.Features.Categories.DTOs;
using Shopfinity.Application.Features.Categories.Services;
using Shopfinity.Domain.Entities;
using Shopfinity.Tests.Helpers;
using Xunit;

namespace Shopfinity.Tests.Features.Categories;

public class CategoryServiceTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        return config.CreateMapper();
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsAllNonDeletedCategories()
    {
        using var ctx = TestDbContextFactory.Create();
        ctx.Categories.AddRange(
            new Category { Id = Guid.NewGuid(), Name = "Clothing", Slug = "clothing", Description = "Apparel", DisplayOrder = 0 },
            new Category { Id = Guid.NewGuid(), Name = "Deleted Cat", Slug = "deleted-cat", Description = "x", IsDeleted = true, DisplayOrder = 0 }
        );
        await ctx.SaveChangesAsync();

        var service = new CategoryService(ctx, CreateMapper());

        var result = (await service.GetAllCategoriesAsync()).ToList();

        // HasData seeds 4 categories + Clothing; soft-deleted row hidden by query filter
        Assert.Equal(5, result.Count);
        Assert.DoesNotContain(result, c => c.Name == "Deleted Cat");
    }

    [Fact]
    public async Task CreateCategoryAsync_PersistsAndReturnsCategory()
    {
        using var ctx = TestDbContextFactory.Create();
        var service = new CategoryService(ctx, CreateMapper());

        var dto = new CreateCategoryDto { Name = "Books", Description = "All kinds of books" };

        var result = await service.CreateCategoryAsync(dto);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Books", result.Name);
        Assert.Equal(5, await ctx.Categories.CountAsync());
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ThrowsWhenNotFound()
    {
        using var ctx = TestDbContextFactory.Create();
        var service = new CategoryService(ctx, CreateMapper());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetCategoryByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ReturnsCorrectCategory()
    {
        using var ctx = TestDbContextFactory.Create();
        var id = Guid.NewGuid();
        ctx.Categories.Add(new Category { Id = id, Name = "Sports", Slug = "sports", Description = "Sporting goods", DisplayOrder = 0 });
        await ctx.SaveChangesAsync();

        var service = new CategoryService(ctx, CreateMapper());
        var result = await service.GetCategoryByIdAsync(id);

        Assert.Equal(id, result.Id);
        Assert.Equal("Sports", result.Name);
    }
}
