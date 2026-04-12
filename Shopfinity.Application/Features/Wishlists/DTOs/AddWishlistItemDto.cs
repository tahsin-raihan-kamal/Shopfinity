using System.ComponentModel.DataAnnotations;

namespace Shopfinity.Application.Features.Wishlists.DTOs;

public class AddWishlistItemDto
{
    [Required]
    public Guid ProductId { get; set; }
}
