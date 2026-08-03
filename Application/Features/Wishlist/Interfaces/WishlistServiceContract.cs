using Application.Features.Wishlist.DTOs;

namespace Application.Features.Wishlist.Interfaces;

public interface WishlistServiceContract
{
    Task<ViewWishlistDto> GetWishlistAsync();
    Task<string> AddProductToWishlistAsync(AddWishlistItemDto dto);
    Task<string> DeleteProductFromWishListAsync(Guid productId);
    Task<string> ClearWishListAsync();
    Task<int> GetWishlistItemsCountAsync();
}