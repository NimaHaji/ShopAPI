using Application.Common.Interfaces;
using Application.Features.Review.DTOs;
using Application.Features.Review.interfaces;
using Domain.Enums;
using Shared.Exceptions;

namespace Application.Features.Review.Implement;

public class ReviewService : ReviewServiceContract
{
    private readonly ReviewsRepositoryContract _reviewsRepositoryContract;
    private readonly UnitOfWorkContract _unitOfWorkContract;

    public ReviewService(ReviewsRepositoryContract reviewsRepositoryContract, UnitOfWorkContract unitOfWorkContract)
    {
        _reviewsRepositoryContract = reviewsRepositoryContract;
        _unitOfWorkContract = unitOfWorkContract;
    }

    public async Task<AdminViewReviewsDto> GetAllReviewsForAdmin()
    {
        var reviews = await _reviewsRepositoryContract.GetAllReviewsForAdmin();

        if (reviews is null)
            return new AdminViewReviewsDto
            {
                Reviews = []
            };

        return new AdminViewReviewsDto
        {
            Reviews = reviews.Select(r => new AdminViewReviewItemDto
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
            }).ToList()
        };
    }

    public async Task<AdminViewReviewItemDto?> GetReviewsByIdForAdmin(Guid reviewId)
    {
        var review = await _reviewsRepositoryContract.GetReviewByIdForAdmin(reviewId);

        if (review is null)
            throw new NotFoundException("نظری یافت نشد .");

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

    public async Task<string> ChangeReviewStatus(Guid reviewId, ReviewStatus status)
    {
        var review = await _reviewsRepositoryContract.GetReviewByIdForAdmin(reviewId);

        if (review is null)
            throw new NotFoundException("نظری یافت نشد .");

        review.ChangeStatus(status);

        await _unitOfWorkContract.SaveAsync();
        return $"وضعیت نظر با موفقیت به {review.ReviewStatus} تغییر کرد .";
    }

    public async Task<string> EditReviewAsAdminAsync(EditReviewAsAdminDto dto,Guid reviewId)
    {
        var review = await _reviewsRepositoryContract.GetReviewByIdForAdmin(reviewId);

        if (review is null)
            throw new NotFoundException("نظری یافت نشد .");

        review.Edit(
            comment: dto.Comment,
            starsCount: dto.StarCount
        );

        await _unitOfWorkContract.SaveAsync();
        return "نظر با موفقیت تغییر یافت";
    }

    public async Task<string> DeleteReviewAsAdminAsync(Guid reviewId)
    {
        var review = await _reviewsRepositoryContract.GetReviewByIdForAdmin(reviewId);

        if (review is null)
            throw new NotFoundException("نظری یافت نشد .");
        
        review.Delete();

        await _unitOfWorkContract.SaveAsync();
        return "نظر با موفقیت حذف شد";
    }

    public async Task<string> RestoreReviewAsAdminAsync(Guid reviewId)
    {
        var review = await _reviewsRepositoryContract.GetReviewByIdForAdmin(reviewId);

        if (review is null)
            throw new NotFoundException("نظری یافت نشد .");
        
        review.Restore();

        await _unitOfWorkContract.SaveAsync();
        return "نظر با موفقیت بازیابی شد";
    }

    public async Task<AdminViewReviewsDto> GetAllReviewsByStatusForAdmin(ReviewStatus status)
    {
        var reviews = await _reviewsRepositoryContract.GetAllReviewsByStatusForAdmin(status);

        if (reviews is null)
            return new AdminViewReviewsDto
            {
                Reviews = []
            };

        return new AdminViewReviewsDto
        {
            Reviews = reviews.Select(r => new AdminViewReviewItemDto
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
            }).ToList()
        };
    }
}