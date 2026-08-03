using Domain.Entities;

namespace Application.Features.Wishlist.Interfaces;

public interface WishlistItemRepositoryContract
{
    Task AddWishlistItem(WishlistItem wishlistItem);
    Task<bool> ExistsAsync(Guid wishlistId, Guid productId);
    Task DeleteWishlistItem(WishlistItem wishlistItem);
    Task<WishlistItem?> GetWishlistItemAsync(Guid wishlistId, Guid productId);
    Task ClearWishlistAsync(Guid wishlistId);
}