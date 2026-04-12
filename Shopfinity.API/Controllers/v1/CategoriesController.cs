using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopfinity.API.Responses;
using Shopfinity.Application.Features.Categories.DTOs;
using Shopfinity.Application.Features.Categories.Services;
using Shopfinity.Domain.Constants;
using System.Threading;

namespace Shopfinity.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _svc;
    public CategoriesController(ICategoryService svc) => _svc = svc;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAll(CancellationToken ct)
    {
        var cats = await _svc.GetAllCategoriesAsync(ct);
        return Ok(ApiResponse<IEnumerable<CategoryDto>>.SuccessResponse(cats));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Get(Guid id, CancellationToken ct)
    {
        var cat = await _svc.GetCategoryByIdAsync(id, ct);
        return Ok(ApiResponse<CategoryDto>.SuccessResponse(cat));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create([FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        var cat = await _svc.CreateCategoryAsync(dto, ct);
        return Created($"/api/v1/categories/{cat.Id}",
            ApiResponse<CategoryDto>.SuccessResponse(cat, "Category created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(Guid id, [FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        var cat = await _svc.UpdateCategoryAsync(id, dto, ct);
        return Ok(ApiResponse<CategoryDto>.SuccessResponse(cat, "Category updated."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        await _svc.DeleteCategoryAsync(id, ct);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Category deleted."));
    }
}
