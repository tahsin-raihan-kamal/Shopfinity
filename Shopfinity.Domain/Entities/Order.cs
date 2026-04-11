using Shopfinity.Domain.Common;
using Shopfinity.Domain.Enums;

namespace Shopfinity.Domain.Entities;

public class Order : BaseEntity
{
    public string      UserId      { get; set; } = string.Empty;
    public decimal     TotalAmount { get; set; }
    public OrderStatus Status      { get; set; } = OrderStatus.Pending;

    public string? IdempotencyKey { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
