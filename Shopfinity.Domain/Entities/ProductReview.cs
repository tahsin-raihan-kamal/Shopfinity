using Shopfinity.Domain.Common;

namespace Shopfinity.Domain.Entities;

public class ProductReview : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty; // Denormalized for rapid display
    public Guid ProductId { get; set; }
    
    public int Rating { get; set; } // 1 to 5 bounds
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    
    // Navigation properties
    public Product Product { get; set; } = null!;
}
