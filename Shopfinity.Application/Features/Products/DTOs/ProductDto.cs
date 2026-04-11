using System.ComponentModel.DataAnnotations;

namespace Shopfinity.Application.Features.Products.DTOs;

public class ProductDto
{
    public Guid    Id            { get; set; }
    public string  Name          { get; set; } = string.Empty;
    public string  Slug          { get; set; } = string.Empty;
    public string  Description   { get; set; } = string.Empty;
    public decimal Price         { get; set; }
    public int     StockQuantity { get; set; }
    public Guid    CategoryId    { get; set; }
    public string? ImageUrl      { get; set; }
    public List<string> Tags     { get; set; } = new();
}

public class CreateProductDto
{
    [Required(ErrorMessage = "Product name is required.")]
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999999.99.")]
    public decimal Price { get; set; }

    [Range(0, 100000, ErrorMessage = "Stock quantity must be between 0 and 100000.")]
    public int StockQuantity { get; set; }

    [Required(ErrorMessage = "CategoryId is required.")]
    public Guid CategoryId { get; set; }
    
    public string? ImageUrl { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class ProductSearchDto
{
    public string?  SearchTerm { get; set; }
    /// <summary>Alias for admin/UI (?search=). Combined with <see cref="SearchTerm"/> (either wins).</summary>
    public string?  Search { get; set; }
    public Guid?    CategoryId { get; set; }
    /// <summary>Filter by category slug (e.g. laptops, smartphones). Ignored if <see cref="CategoryId"/> is set.</summary>
    public string?  CategorySlug { get; set; }
    public decimal? MinPrice   { get; set; }
    public decimal? MaxPrice   { get; set; }
    
    public string? SortBy { get; set; } 

    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be >= 1.")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 10;
}
