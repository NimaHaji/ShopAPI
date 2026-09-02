using Application.Common.Interfaces;
using Application.Common.Observability;
using Application.Features.Review.DTOs;
using Application.Features.Review.interfaces;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Exceptions;

namespace Application.Features.Review.Implement;

public class ReviewService : ReviewServiceContract
{
    private readonly ReviewsRepositoryContract _reviewsRepositoryContract;
    private readonly UnitOfWorkContract _unitOfWorkContract;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(ReviewsRepositoryContract reviewsRepositoryContract, UnitOfWorkContract unitOfWorkContract,
        ILogger<ReviewService> logger)
    {
        _reviewsRepositoryContract = reviewsRepositoryContract;
        _unitOfWorkContract = unitOfWorkContract;
        _logger = logger;
    }

    public async Task<AdminViewReviewsDto> GetAllReviewsForAdmin()
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(GetAllReviewsForAdmin));

        var reviews =
            await _reviewsRepositoryContract
                .GetAllReviewsForAdmin();

        var result = reviews?.Select(r => new AdminViewReviewItemDto
        {
            Id = r.Id,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            DeletedAt = r.DeletedAt,
            ProductId = r.ProductId,
            StarsCount = r.StarsCount,
            UpdatedAt = r.UpdatedAt,
            ProductTitle = r.Product.Title,
            IsDeleted = r.IsDeleted,
            ReviewStatus = r.ReviewStatus.ToString(),
            User = new AdminViewReviewItemUserDto
            {
                Id = r.UserId,
                Name = r.User.FullName
            }
        }).ToList() ?? [];

        activity?.SetTag("reviews.count", result.Count);

        return new AdminViewReviewsDto
        {
            Reviews = result
        };
    }

    public async Task<AdminViewReviewItemDto?> GetReviewsByIdForAdmin(
        Guid reviewId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(GetReviewsByIdForAdmin));

        activity?.SetTag("review.id", reviewId);

        var review =
            await _reviewsRepositoryContract
                .GetReviewByIdForAdmin(reviewId);

        if (review is null)
        {
            _logger.LogWarning(
                LogEvents.Review.NotFound,
                "Review was not found. ReviewId: {ReviewId}",
                reviewId);

            throw new NotFoundException("نظری یافت نشد .");
        }

        return new AdminViewReviewItemDto
        {
            Id = review.Id,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            DeletedAt = review.DeletedAt,
            ProductId = review.ProductId,
            StarsCount = review.StarsCount,
            UpdatedAt = review.UpdatedAt,
            ProductTitle = review.Product.Title,
            IsDeleted = review.IsDeleted,
            ReviewStatus = review.ReviewStatus.ToString(),
            User = new AdminViewReviewItemUserDto
            {
                Id = review.UserId,
                Name = review.User.FullName
            }
        };
    }

    public async Task<string> ChangeReviewStatus(
        Guid reviewId,
        ReviewStatus status)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(ChangeReviewStatus));

        activity?.SetTag("review.id", reviewId);
        activity?.SetTag("review.status", status.ToString());

        var review =
            await _reviewsRepositoryContract
                .GetReviewByIdForAdmin(reviewId);

        if (review is null)
        {
            _logger.LogWarning(
                LogEvents.Review.NotFound,
                "Review status change rejected because review was not found. ReviewId: {ReviewId}",
                reviewId);

            throw new NotFoundException("نظری یافت نشد .");
        }

        review.ChangeStatus(status);

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Review.StatusChanged,
            "Review status changed successfully. ReviewId: {ReviewId}, Status: {Status}",
            reviewId,
            status);

        return $"وضعیت نظر با موفقیت به {review.ReviewStatus} تغییر کرد .";
    }

    public async Task<string> EditReviewAsAdminAsync(
        EditReviewAsAdminDto dto,
        Guid reviewId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(EditReviewAsAdminAsync));

        activity?.SetTag("review.id", reviewId);
        activity?.SetTag("review.stars", dto.StarCount);

        _logger.LogInformation(
            LogEvents.Review.EditStarted,
            "Admin review edit started. ReviewId: {ReviewId}",
            reviewId);

        var review =
            await _reviewsRepositoryContract
                .GetReviewByIdForAdmin(reviewId);

        if (review is null)
        {
            _logger.LogWarning(
                LogEvents.Review.NotFound,
                "Review edit rejected because review was not found. ReviewId: {ReviewId}",
                reviewId);

            throw new NotFoundException("نظری یافت نشد .");
        }

        review.Edit(
            comment: dto.Comment,
            starsCount: dto.StarCount);

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Review.EditCompleted,
            "Review edited successfully. ReviewId: {ReviewId}",
            reviewId);

        return "نظر با موفقیت تغییر یافت";
    }

    public async Task<string> DeleteReviewAsAdminAsync(Guid reviewId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(DeleteReviewAsAdminAsync));

        activity?.SetTag("review.id", reviewId);

        _logger.LogInformation(
            LogEvents.Review.DeleteStarted,
            "Admin review deletion started. ReviewId: {ReviewId}",
            reviewId);

        var review =
            await _reviewsRepositoryContract
                .GetReviewByIdForAdmin(reviewId);

        if (review is null)
        {
            _logger.LogWarning(
                LogEvents.Review.NotFound,
                "Review deletion rejected because review was not found. ReviewId: {ReviewId}",
                reviewId);

            throw new NotFoundException("نظری یافت نشد .");
        }

        review.Delete();

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Review.DeleteCompleted,
            "Review deleted successfully. ReviewId: {ReviewId}",
            reviewId);

        return "نظر با موفقیت حذف شد";
    }

    public async Task<string> RestoreReviewAsAdminAsync(Guid reviewId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(RestoreReviewAsAdminAsync));

        activity?.SetTag("review.id", reviewId);

        _logger.LogInformation(
            LogEvents.Review.RestoreStarted,
            "Admin review restore started. ReviewId: {ReviewId}",
            reviewId);

        var review =
            await _reviewsRepositoryContract
                .GetReviewByIdForAdmin(reviewId);

        if (review is null)
        {
            _logger.LogWarning(
                LogEvents.Review.NotFound,
                "Review restore rejected because review was not found. ReviewId: {ReviewId}",
                reviewId);

            throw new NotFoundException("نظری یافت نشد .");
        }

        review.Restore();

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Review.RestoreCompleted,
            "Review restored successfully. ReviewId: {ReviewId}",
            reviewId);

        return "نظر با موفقیت بازیابی شد";
    }

    public async Task<AdminViewReviewsDto> GetAllReviewsByStatusForAdmin(
        ReviewStatus status)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(GetAllReviewsByStatusForAdmin));

        activity?.SetTag("review.status", status.ToString());

        var reviews =
            await _reviewsRepositoryContract
                .GetAllReviewsByStatusForAdmin(status);

        var result = reviews?.Select(r => new AdminViewReviewItemDto
        {
            Id = r.Id,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            DeletedAt = r.DeletedAt,
            ProductId = r.ProductId,
            StarsCount = r.StarsCount,
            UpdatedAt = r.UpdatedAt,
            ProductTitle = r.Product.Title,
            IsDeleted = r.IsDeleted,
            ReviewStatus = r.ReviewStatus.ToString(),
            User = new AdminViewReviewItemUserDto
            {
                Id = r.UserId,
                Name = r.User.FullName
            }
        }).ToList() ?? [];

        activity?.SetTag("reviews.count", result.Count);

        return new AdminViewReviewsDto
        {
            Reviews = result
        };
    }
}