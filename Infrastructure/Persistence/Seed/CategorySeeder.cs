using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Seed.Models;

namespace Infrastructure.Persistence.Seed;

public class CategorySeeder
{
    private readonly ShopDbContext _context;
    private readonly JsonSeedReader _reader;

    public CategorySeeder(ShopDbContext context, JsonSeedReader reader)
    {
        _context = context;
        _reader = reader;
    }

    public async Task SeedAsync(SeedContext seedContext)
    {
        var items = await _reader.ReadListAsync<CategorySeedDto>("categories.json", required: true);

        foreach (var item in items)
        {
            var category = ProductCategory.Create(item.Title);
            seedContext.Categories[item.Key] = category.Id;
            await _context.ProductCategories.AddAsync(category);
        }
    }
}
