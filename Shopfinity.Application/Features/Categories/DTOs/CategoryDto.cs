using System.ComponentModel.DataAnnotations;

namespace Shopfinity.Application.Features.Categories.DTOs;

public class CategoryDto
{
    public Guid   Id          { get; set; }
    public string Name        { get; set; } = string.Empty;
    public string Slug        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl   { get; set; }
    public int DisplayOrder   { get; set; }
}

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Category name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
}
