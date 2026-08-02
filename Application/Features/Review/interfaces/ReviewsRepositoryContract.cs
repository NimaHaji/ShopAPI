using Application.Features.Review.DTOs;
using Domain.Enums;

namespace Application.Features.Review.interfaces;

public interface ReviewsRepositoryContract
{
    Task<List<Domain.Entities.Review>?> GetAllReviewsByProductId(Guid productId);
    Task<bool> ExistsByUserAndProductAsync(Guid productId, Guid userId);
    Task AddReview(Domain.Entities.Review review);
    Task<List<Domain.Entities.Review>?> GetAllReviewsForAdmin();
    Task<Domain.Entities.Review?> GetReviewByIdForAdmin(Guid reviewId);
    Task<List<Domain.Entities.Review>?> GetAllReviewsByStatusForAdmin(ReviewStatus status);
    Task<(decimal AverageRating, int ReviewCount)> GetProductRatingAsync(Guid productId);
}