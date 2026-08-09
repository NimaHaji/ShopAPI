using Application.Features.Wishlist.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class WishlistItemRepository:WishlistItemRepositoryContract
{
    private readonly ShopDbContext _shopDbContext;

    public WishlistItemRepository(ShopDbContext shopDbContext)
    {
        _shopDbContext = shopDbContext;
    }

    public async Task AddWishlistItem(WishlistItem wishlistItem)
    {
        await _shopDbContext
            .WishlistsItems
            .AddAsync(wishlistItem);
    }

    public async Task<bool> ExistsAsync(Guid wishlistId, Guid productId)
    {
        return await _shopDbContext
            .WishlistsItems
            .AnyAsync(w => w.Id == wishlistId && w.ProductId == productId);
    }

    public async Task DeleteWishlistItem(WishlistItem wishlistItem)
    {
         _shopDbContext
            .WishlistsItems
            .Remove(wishlistItem);
    }
    public async Task<WishlistItem?> GetWishlistItemAsync(Guid wishlistId, Guid productId)
    {
        return await _shopDbContext.WishlistsItems
            .FirstOrDefaultAsync(x =>
                x.WishlistId == wishlistId &&
                x.ProductId == productId);
    }

    public async Task ClearWishlistAsync(Guid wishlistId)
    {
        await _shopDbContext.WishlistsItems
            .Where(x => x.WishlistId == wishlistId)
            .ExecuteDeleteAsync();
    }
}