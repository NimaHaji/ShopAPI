using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Seed.Models;

namespace Infrastructure.Persistence.Seed;

public class CouponSeeder
{
    private readonly ShopDbContext _context;
    private readonly JsonSeedReader _reader;

    public CouponSeeder(ShopDbContext context, JsonSeedReader reader)
    {
        _context = context;
        _reader = reader;
    }

    public async Task SeedAsync(SeedContext seedContext)
    {
        var items = await _reader.ReadListAsync<CouponSeedDto>("coupons.json");

        foreach (var item in items)
        {
            var discountType = ParseDiscountType(item.DiscountType);
            var coupon = new Coupon(
                item.Code,
                discountType,
                item.Value,
                item.StartsAt,
                item.EndAt,
                item.MinimumOrderAmount,
                item.MaxDiscountAmount,
                item.UsageLimit,
                item.UserUsageLimit);

            coupon.Activate();
            seedContext.Coupons[item.Key] = coupon.Id;
            await _context.Coupons.AddAsync(coupon);
        }
    }

    private static DiscountType ParseDiscountType(string value) =>
        Enum.Parse<DiscountType>(value, ignoreCase: true);
}
