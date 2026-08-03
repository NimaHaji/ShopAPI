using Application.Features.CartItem.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;

namespace Infrastructure.Persistence.Repositories;

public class CartItemRepository:CartItemRepositoryContract
{
    private readonly ShopDbContext _shopDbContext;

    public CartItemRepository(ShopDbContext shopDbContext)
    {
        _shopDbContext = shopDbContext;
    }

    public async Task AddCartItemAsync(CartItem cartItem)
    {
        await _shopDbContext.CartItems.AddAsync(cartItem);  
    }
}