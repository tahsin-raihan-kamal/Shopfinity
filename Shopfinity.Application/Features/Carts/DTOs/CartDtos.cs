using System.ComponentModel.DataAnnotations;

namespace Shopfinity.Application.Features.Carts.DTOs;

public class CartResponseDto
{
    public Guid Id { get; set; }
    public ICollection<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    public decimal TotalPrice => Items.Sum(i => i.UnitPrice * i.Quantity);
}

public class CartItemDto
{
    public Guid    Id          { get; set; }
    public Guid    ProductId   { get; set; }
    public string  ProductSlug { get; set; } = string.Empty;
    public string  ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int     Quantity    { get; set; }
    public decimal UnitPrice   { get; set; }
    public decimal Subtotal    => UnitPrice * Quantity;
}

public class AddToCartDto
{
    [Required(ErrorMessage = "ProductId is required.")]
    public Guid ProductId { get; set; }

    [Range(1, 9999, ErrorMessage = "Quantity must be between 1 and 9999.")]
    public int Quantity { get; set; }
}
