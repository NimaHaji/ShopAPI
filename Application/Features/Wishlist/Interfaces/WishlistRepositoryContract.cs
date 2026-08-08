namespace Application.Features.Wishlist.Interfaces;

public interface WishlistRepositoryContract
{
    Task AddWishlistAsync(Domain.Entities.Wishlist wishlist);
    Task<Domain.Entities.Wishlist?> GetWishlistByUserId(Guid userId);
}