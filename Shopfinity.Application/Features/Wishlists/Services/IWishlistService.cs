using Shopfinity.Application.Features.Wishlists.DTOs;
using System.Threading;

namespace Shopfinity.Application.Features.Wishlists.Services;

public interface IWishlistService
{
    Task<IEnumerable<WishlistItemDto>> GetUserWishlistAsync(string userId, CancellationToken ct = default);
    Task<WishlistItemDto> AddItemAsync(string userId, AddWishlistItemDto dto, CancellationToken ct = default);
    Task RemoveItemAsync(string userId, Guid id, CancellationToken ct = default);
}
