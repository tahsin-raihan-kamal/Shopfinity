using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shopfinity.API.Responses;
using Shopfinity.Application.Features.Reviews.DTOs;
using Shopfinity.Application.Features.Reviews.Services;
using System.Security.Claims;
using System.Threading;

namespace Shopfinity.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
    private string GetUserName() => User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Anonymous";

    [HttpGet("product/{productId:guid}")]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new string[] { "*" })] // 1 minute cache
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductReviewDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductReviewDto>>>> GetProductReviews(Guid productId, CancellationToken ct)
    {
        var reviews = await _reviewService.GetProductReviewsAsync(productId, ct);
        return Ok(ApiResponse<IEnumerable<ProductReviewDto>>.SuccessResponse(reviews));
    }

    [HttpPost]
    [Authorize]
    [EnableRateLimiting("ReviewSubmissionPolicy")]
    [ProducesResponseType(typeof(ApiResponse<ProductReviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProductReviewDto>>> AddReview([FromBody] CreateReviewDto dto, CancellationToken ct)
    {
        var review = await _reviewService.AddReviewAsync(GetUserId(), GetUserName(), dto, ct);
        _logger.LogInformation("User {UserId} added review for product {ProductId}.", GetUserId(), dto.ProductId);
        return Ok(ApiResponse<ProductReviewDto>.SuccessResponse(review, "Review submitted successfully."));
    }
}
