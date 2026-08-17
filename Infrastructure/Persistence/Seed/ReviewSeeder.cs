using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Seed.Models;

namespace Infrastructure.Persistence.Seed;

public class ReviewSeeder
{
    private readonly ShopDbContext _context;
    private readonly JsonSeedReader _reader;

    public ReviewSeeder(ShopDbContext context, JsonSeedReader reader)
    {
        _context = context;
        _reader = reader;
    }

    public async Task SeedAsync(SeedContext seedContext)
    {
        var items = await _reader.ReadListAsync<ReviewSeedDto>("reviews.json");

        foreach (var item in items)
        {
            if (!seedContext.Users.TryGetValue(item.UserKey, out var userId))
                throw new InvalidOperationException($"User key not found for review: {item.UserKey}");

            if (!seedContext.Products.TryGetValue(item.ProductKey, out var productId))
                throw new InvalidOperationException($"Product key not found for review: {item.ProductKey}");

            var review = new Review(item.StarsCount, item.Comment, userId, productId);
            review.ChangeStatus(ParseReviewStatus(item.Status));
            await _context.Reviews.AddAsync(review);
        }
    }

    private static ReviewStatus ParseReviewStatus(string value) =>
        Enum.Parse<ReviewStatus>(value, ignoreCase: true);
}
