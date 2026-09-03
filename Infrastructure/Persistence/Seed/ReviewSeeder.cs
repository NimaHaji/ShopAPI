using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Seed.Models;
using Microsoft.EntityFrameworkCore;

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

        // Deduplicate seed rows by natural key (UserId, ProductId) so the JSON
        // itself can never violate the unique index, and re-runs stay idempotent.
        var seen = new HashSet<(Guid UserId, Guid ProductId)>();

        foreach (var item in items)
        {
            if (!seedContext.Users.TryGetValue(item.UserKey, out var userId))
                throw new InvalidOperationException($"User key not found for review: {item.UserKey}");

            if (!seedContext.Products.TryGetValue(item.ProductKey, out var productId))
                throw new InvalidOperationException($"Product key not found for review: {item.ProductKey}");

            if (!seen.Add((userId, productId)))
                continue;

            var exists = await _context.Reviews.AnyAsync(r =>
                r.UserId == userId && r.ProductId == productId);

            if (exists)
                continue;

            var review = new Review(item.StarsCount, item.Comment, userId, productId);
            review.ChangeStatus(ParseReviewStatus(item.Status));
            await _context.Reviews.AddAsync(review);
        }
    }

    private static ReviewStatus ParseReviewStatus(string value) =>
        Enum.Parse<ReviewStatus>(value, ignoreCase: true);
}
