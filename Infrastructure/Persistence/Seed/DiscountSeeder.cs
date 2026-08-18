using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Seed.Models;

namespace Infrastructure.Persistence.Seed;

public class DiscountSeeder
{
    private readonly ShopDbContext _context;
    private readonly JsonSeedReader _reader;

    public DiscountSeeder(ShopDbContext context, JsonSeedReader reader)
    {
        _context = context;
        _reader = reader;
    }

    public async Task SeedAsync(SeedContext seedContext)
    {
        var items = await _reader.ReadListAsync<DiscountSeedDto>("discounts.json");

        foreach (var item in items)
        {
            var discountType = ParseDiscountType(item.DiscountType);
            var discount = new Discount(
                item.Title,
                discountType,
                item.Value,
                item.MaxDiscountAmount,
                item.StartsAt,
                item.EndsAt);

            seedContext.Discounts[item.Key] = discount.Id;
            await _context.Discounts.AddAsync(discount);

            foreach (var productKey in item.ProductKeys)
            {
                if (!seedContext.Products.TryGetValue(productKey, out var productId))
                    throw new InvalidOperationException($"Product key not found for discount: {productKey}");

                await _context.DiscountProducts.AddAsync(new DiscountProduct(discount.Id, productId));
            }

            foreach (var variantKey in item.VariantKeys)
            {
                if (!seedContext.Variants.TryGetValue(variantKey, out var variantId))
                    throw new InvalidOperationException($"Variant key not found for discount: {variantKey}");

                await _context.DiscountVariants.AddAsync(DiscountVariant.Create(discount.Id, variantId));
            }
        }
    }

    private static DiscountType ParseDiscountType(string value) =>
        Enum.Parse<DiscountType>(value, ignoreCase: true);
}
