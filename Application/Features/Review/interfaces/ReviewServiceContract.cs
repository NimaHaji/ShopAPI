using Application.Features.Review.DTOs;
using Domain.Enums;

namespace Application.Features.Review.interfaces;

public interface ReviewServiceContract
{
    Task<AdminViewReviewsDto> GetAllReviewsForAdmin();
    Task<AdminViewReviewItemDto?> GetReviewsByIdForAdmin(Guid reviewId);
    Task<string> ChangeReviewStatus(Guid reviewId, ReviewStatus status);
    Task<string> EditReviewAsAdminAsync(EditReviewAsAdminDto dto,Guid reviewId);
    Task<string> DeleteReviewAsAdminAsync(Guid reviewId);
    Task<string> RestoreReviewAsAdminAsync(Guid reviewId);
    Task<AdminViewReviewsDto> GetAllReviewsByStatusForAdmin(ReviewStatus status);
}