using Shopfinity.Application.Features.Reviews.DTOs;
using System.Threading;

namespace Shopfinity.Application.Features.Reviews.Services;

public interface IReviewService
{
    Task<IEnumerable<ProductReviewDto>> GetProductReviewsAsync(Guid productId, CancellationToken ct = default);
    Task<ProductReviewDto> AddReviewAsync(string userId, string userName, CreateReviewDto dto, CancellationToken ct = default);
}
