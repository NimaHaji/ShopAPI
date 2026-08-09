namespace Application.Features.CartItem.Interfaces;

public interface CartItemRepositoryContract
{
    Task AddCartItemAsync(Domain.Entities.CartItem cartItem);
}