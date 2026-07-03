using Domain.Entites;

namespace Application.Features.Cart.Interfaces;

public interface CartRepositoryContract
{
    Task<Domain.Entites.Cart?> GetCartByUserIdAsync(Guid userId);
    Task<Domain.Entites.Cart?> GetCartWithProductsByUserIdAsync(Guid userId);
    Task<CartItem?> GetCartItemByProductIdAsync(Guid productId);
    
    Task CreateCartAsync(Domain.Entites.Cart cart);
    Task SaveAsync();
}