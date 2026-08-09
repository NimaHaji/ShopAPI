using Application.Features.Review.interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ReviewsRepository : ReviewsRepositoryContract
{
    private readonly ShopDbContext _shopDbContext;

    public ReviewsRepository(ShopDbContext shopDbContext)
    {
        _shopDbContext = shopDbContext;
    }

    public async Task<List<Review>?> GetAllReviewsByProductId(Guid productId)
    {
        return await _shopDbContext
            .Reviews
            .Include(x => x.User)
            .Where(r =>
                r.ReviewStatus == ReviewStatus.Approved &&
                !r.IsDeleted &&
                r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsByUserAndProductAsync(Guid productId, Guid userId)
    {
        return await _shopDbContext
            .Reviews
            .Where(r => 
                !r.IsDeleted)
            .AnyAsync(r => r.UserId == userId &&
                           r.ProductId == productId);
    }

    public async Task AddReview(Review review)
    {
        await _shopDbContext
            .Reviews
            .AddAsync(review);
    }

    public async Task<List<Review>?> GetAllReviewsForAdmin()
    {
        return await _shopDbContext
            .Reviews
            .Include(x => x.User)
            .Include(x => x.Product)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review?> GetReviewByIdForAdmin(Guid reviewId)
    {
        return await _shopDbContext
            .Reviews
            .Include(x => x.User)
            .Include(x => x.Product)
            .Where(r=>r.Id == reviewId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Review>?> GetAllReviewsByStatusForAdmin(ReviewStatus status)
    {
        return await _shopDbContext
            .Reviews
            .Include(x => x.User)
            .Include(x => x.Product)
            .Where(r => r.ReviewStatus == status)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<(decimal AverageRating, int ReviewCount)> GetProductRatingAsync(
        Guid productId)
    {
        var query = _shopDbContext
            .Reviews
            .Where(x =>
                x.ProductId == productId &&
                !x.IsDeleted &&
                x.ReviewStatus == ReviewStatus.Approved);

        var count = await query.CountAsync();

        if (count == 0)
            return (0, 0);

        var average = await query.AverageAsync(x => (decimal)x.StarsCount);

        return (Math.Round(average, 2), count);
    }
}