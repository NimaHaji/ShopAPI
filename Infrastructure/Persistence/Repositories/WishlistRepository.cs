using Application.Features.Wishlist.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class WishlistRepository:WishlistRepositoryContract
{
    private readonly ShopDbContext _shopDbContext;

    public WishlistRepository(ShopDbContext shopDbContext)
    {
        _shopDbContext = shopDbContext;
    }

    public async Task AddWishlistAsync(Wishlist wishlist)
    {
        await _shopDbContext
            .Wishlists
            .AddAsync(wishlist);
    }

    public async Task<Wishlist?> GetWishlistByUserId(Guid userId)
    {
        return await _shopDbContext
            .Wishlists
            .Include(w => w.WishlistItems)
            .ThenInclude(wi => wi.Product)
            .ThenInclude(p=>p.Images)
            .Include(w=>w.WishlistItems)
            .ThenInclude(wi => wi.Product)
            .ThenInclude(p=>p.Variants)
            .Where(w => w.UserId == userId)
            .FirstOrDefaultAsync();
    }
}