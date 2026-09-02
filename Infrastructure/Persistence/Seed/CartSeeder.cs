using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Seed.Models;

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

            foreach (var item in cartDto.Items)
            {
                if (!seedContext.Variants.TryGetValue(item.VariantKey, out var variantId))
                    throw new InvalidOperationException($"Variant key not found for cart: {item.VariantKey}");

                await _context.CartItems.AddAsync(new CartItem(cartId, variantId, item.Quantity));
            }
        }
    }
}
