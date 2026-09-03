using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Seed.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seed;

public class BrandSeeder
{
    private readonly ShopDbContext _context;
    private readonly JsonSeedReader _reader;

    public BrandSeeder(ShopDbContext context, JsonSeedReader reader)
    {
        _context = context;
        _reader = reader;
    }

    public async Task SeedAsync(SeedContext seedContext)
    {
        var items = await _reader.ReadListAsync<BrandSeedDto>("brands.json", required: true);

        foreach (var item in items)
        {
            var existing = await _context.ProductBrands
                .FirstOrDefaultAsync(b => b.Title == item.Title);

            if (existing is not null)
            {
                seedContext.Brands[item.Key] = existing.Id;
                continue;
            }

            var brand = ProductBrand.Create(item.Title);
            seedContext.Brands[item.Key] = brand.Id;
            await _context.ProductBrands.AddAsync(brand);
        }
    }
}
