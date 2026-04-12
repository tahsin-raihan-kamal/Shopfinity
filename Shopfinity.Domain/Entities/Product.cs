using Shopfinity.Domain.Common;

namespace Shopfinity.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public List<string> Tags { get; set; } = new();

    // Computed Postgres Search Vector
    public NpgsqlTypes.NpgsqlTsVector? SearchVector { get; set; }

    // Concurrency Token
    public byte[]? RowVersion { get; set; }

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
}
