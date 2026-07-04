using Domain.Entities;

namespace Application.Features.Cart.Interfaces;

public interface CartRepositoryContract
{
    Task<Domain.Entities.Cart?> GetCartByUserIdAsync(Guid userId);
    Task<Domain.Entities.Cart?> GetCartWithProductsByUserIdAsync(Guid userId);
    Task<CartItem?> GetCartItemByProductIdAsync(Guid productId);
    
    Task CreateCartAsync(Domain.Entities.Cart cart);
    Task SaveAsync();
}