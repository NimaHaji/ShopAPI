using Application.Features.Cart.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CartRepository:CartRepositoryContract
{
    private readonly ShopDbContext _context;

    public CartRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetCartByUserIdAsync(Guid userId)
    {
        return await _context
            .Carts
            .Include(c => c.CartItems)
            .ThenInclude(c => c.Product)
            .Where(c => c.UserId == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<Cart?> GetCartWithProductsByUserIdAsync(Guid userId)
    {
        return await _context
            .Carts
            .Include(c => c.CartItems)
            .ThenInclude(c => c.Product)
            .FirstOrDefaultAsync(c=>c.UserId == userId);
    }

    public async Task<CartItem?> GetCartItemByProductIdAsync(Guid productId)
    {
        return await _context
            .CartItems
            .Where(ci => ci.ProductId == productId)
            .FirstOrDefaultAsync();
    }

    public async Task CreateCartAsync(Cart cart)
    {
        await _context.Carts.AddAsync(cart);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}