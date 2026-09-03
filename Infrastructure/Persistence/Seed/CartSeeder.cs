using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Seed.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seed;

public class CartSeeder
{
    private readonly ShopDbContext _context;
    private readonly JsonSeedReader _reader;

    public CartSeeder(ShopDbContext context, JsonSeedReader reader)
    {
        _context = context;
        _reader = reader;
    }

    public async Task SeedAsync(SeedContext seedContext)
    {
        var items = await _reader.ReadListAsync<CartSeedDto>("carts.json");

        foreach (var cartDto in items)
        {
            if (!seedContext.Carts.TryGetValue(cartDto.UserKey, out var cartId))
                throw new InvalidOperationException($"Cart not found for user: {cartDto.UserKey}");

            // Aggregate duplicates inside the JSON (e.g. same variant listed twice)
            // by summing quantities, otherwise the unique index
            // (CartId, ProductVariantId) fails on the very first run.
            var aggregated = cartDto.Items
                .GroupBy(i => i.VariantKey, StringComparer.OrdinalIgnoreCase)
                .Select(g => new
                {
                    VariantKey = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                });

            foreach (var item in aggregated)
            {
                if (!seedContext.Variants.TryGetValue(item.VariantKey, out var variantId))
                    throw new InvalidOperationException($"Variant key not found for cart: {item.VariantKey}");

                // Idempotency: check tracked entities first (same run), then DB.
                var tracked = _context.CartItems.Local
                    .FirstOrDefault(ci => ci.CartId == cartId && ci.ProductVariantId == variantId);

                if (tracked is not null)
                {
                    tracked.SetQuantity(tracked.Quantity + item.Quantity);
                    continue;
                }

                var existing = await _context.CartItems.FirstOrDefaultAsync(ci =>
                    ci.CartId == cartId && ci.ProductVariantId == variantId);

                if (existing is not null)
                    continue;

                await _context.CartItems.AddAsync(new CartItem(cartId, variantId, item.Quantity));
            }
        }
    }
}
