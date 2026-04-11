using Shopfinity.Domain.Common;

namespace Shopfinity.Domain.Entities;

public class WishlistItem : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid ProductId { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
}
