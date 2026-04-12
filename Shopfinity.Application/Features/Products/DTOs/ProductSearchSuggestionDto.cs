namespace Shopfinity.Application.Features.Products.DTOs;

public class ProductSearchSuggestionDto
{
    public Guid    Id       { get; set; }
    public string  Name     { get; set; } = string.Empty;
    public decimal Price    { get; set; }
    public string? ImageUrl { get; set; }
    public string  Slug     { get; set; } = string.Empty;
}
