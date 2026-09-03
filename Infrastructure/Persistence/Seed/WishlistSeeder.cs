using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Seed.Models;
using Microsoft.EntityFrameworkCore;

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

            // Deduplicate JSON entries (same product listed twice) + skip existing rows.
            var distinctKeys = wishlistDto.ProductKeys
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var productKey in distinctKeys)
            {
                if (!seedContext.Products.TryGetValue(productKey, out var productId))
                    throw new InvalidOperationException($"Product key not found for wishlist: {productKey}");

                var alreadyTracked = _context.WishlistsItems.Local
                    .Any(wi => wi.WishlistId == wishlistId && wi.ProductId == productId);

                if (alreadyTracked)
                    continue;

                var exists = await _context.WishlistsItems.AnyAsync(wi =>
                    wi.WishlistId == wishlistId && wi.ProductId == productId);

                if (exists)
                    continue;

                await _context.WishlistsItems.AddAsync(new WishlistItem(productId, wishlistId));
            }
        }
    }
}
