using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shopfinity.API.Responses;
using Shopfinity.Application.Common;
using Shopfinity.Application.Features.Products.DTOs;
using Shopfinity.Application.Features.Products.Services;
using Shopfinity.Domain.Constants;
using System.Collections.Generic;
using System.Threading;

namespace Shopfinity.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _svc;
    public ProductsController(IProductService svc) => _svc = svc;

    [HttpGet]
    [EnableRateLimiting("SearchRatePolicy")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<ProductDto>>>> Search(
        [FromQuery] ProductSearchDto searchDto, CancellationToken ct)
    {
        var result = await _svc.SearchProductsAsync(searchDto, ct);
        return Ok(ApiResponse<PaginatedResult<ProductDto>>.SuccessResponse(result));
    }

    /// <summary>Navbar / typeahead: ranked suggestions (max 10).</summary>
    [HttpGet("search")]
    [EnableRateLimiting("SearchRatePolicy")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProductSearchSuggestionDto>>>> SearchSuggestions(
        [FromQuery] string? q, CancellationToken ct)
    {
        var list = await _svc.SearchSuggestionsAsync(q, ct);
        return Ok(ApiResponse<IReadOnlyList<ProductSearchSuggestionDto>>.SuccessResponse(list));
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Get(string slug, CancellationToken ct)
    {
        var product = await _svc.GetProductBySlugAsync(slug, ct);
        return Ok(ApiResponse<ProductDto>.SuccessResponse(product));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductDto dto, CancellationToken ct)
    {
        var product = await _svc.CreateProductAsync(dto, ct);
        return Created($"/api/v1/products/{product.Slug}",
            ApiResponse<ProductDto>.SuccessResponse(product, "Product created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(Guid id, [FromBody] CreateProductDto dto, CancellationToken ct)
    {
        var product = await _svc.UpdateProductAsync(id, dto, ct);
        return Ok(ApiResponse<ProductDto>.SuccessResponse(product, "Product updated."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        await _svc.DeleteProductAsync(id, ct);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Product deleted."));
    }
}
