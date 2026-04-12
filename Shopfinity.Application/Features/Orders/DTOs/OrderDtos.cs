using Shopfinity.Domain.Enums;

namespace Shopfinity.Application.Features.Orders.DTOs;

public class OrderResponseDto
{
    public Guid        Id          { get; set; }
    public OrderStatus Status      { get; set; }
    public decimal     TotalAmount { get; set; }
    public DateTime    CreatedAt   { get; set; }
    public ICollection<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
}

public class OrderItemDto
{
    public Guid    ProductId   { get; set; }
    public string  ProductName { get; set; } = string.Empty;
    public int     Quantity    { get; set; }
    public decimal UnitPrice   { get; set; }
}

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}
