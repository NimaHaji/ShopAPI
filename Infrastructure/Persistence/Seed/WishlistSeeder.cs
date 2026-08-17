using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Seed.Models;

namespace Infrastructure.Persistence.Seed;

public class WishlistSeeder
{
    private readonly ShopDbContext _context;
    private readonly JsonSeedReader _reader;

    public WishlistSeeder(ShopDbContext context, JsonSeedReader reader)
    {
        _context = context;
        _reader = reader;
    }

    public async Task SeedAsync(SeedContext seedContext)
    {
        var items = await _reader.ReadListAsync<WishlistSeedDto>("wishlists.json");

        foreach (var wishlistDto in items)
        {
            if (!seedContext.Wishlists.TryGetValue(wishlistDto.UserKey, out var wishlistId))
                throw new InvalidOperationException($"Wishlist not found for user: {wishlistDto.UserKey}");

            foreach (var productKey in wishlistDto.ProductKeys)
            {
                if (!seedContext.Products.TryGetValue(productKey, out var productId))
                    throw new InvalidOperationException($"Product key not found for wishlist: {productKey}");

                await _context.WishlistsItems.AddAsync(new WishlistItem(productId, wishlistId));
            }
        }
    }
}
