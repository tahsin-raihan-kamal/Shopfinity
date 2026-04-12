using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Shopfinity.Application.Common.Interfaces;
using Shopfinity.Application.Features.Reviews.DTOs;
using Shopfinity.Domain.Entities;
using System.Threading;

namespace Shopfinity.Application.Features.Reviews.Services;

public class ReviewService : IReviewService
{
    private readonly IAppDbContext _db;

    public ReviewService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ProductReviewDto>> GetProductReviewsAsync(Guid productId, CancellationToken ct = default)
    {
        return await _db.ProductReviews
            .Where(x => x.ProductId == productId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(r => new ProductReviewDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.UserName,
                ProductId = r.ProductId,
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<ProductReviewDto> AddReviewAsync(string userId, string userName, CreateReviewDto dto, CancellationToken ct = default)
    {
        var product = await _db.Products.FindAsync(new object[] { dto.ProductId }, ct);
        if (product == null) throw new KeyNotFoundException("Product not found.");

        var existing = await _db.ProductReviews.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == dto.ProductId, ct);
        if (existing != null) throw new InvalidOperationException("You have already reviewed this product.");

        var review = new ProductReview
        {
            UserId = userId,
            UserName = userName,
            ProductId = dto.ProductId,
            Rating = dto.Rating,
            Title = dto.Title,
            Comment = dto.Comment
        };

        _db.ProductReviews.Add(review);
        await _db.SaveChangesAsync(ct);

        return new ProductReviewDto
        {
            Id = review.Id,
            UserId = userId,
            UserName = userName,
            ProductId = review.ProductId,
            Rating = review.Rating,
            Title = review.Title,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }
}
