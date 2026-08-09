using Application.Features.Review.DTOs;
using Application.Features.Review.interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly ReviewServiceContract _reviewServiceContract;

    public ReviewsController(ReviewServiceContract reviewServiceContract)
    {
        _reviewServiceContract = reviewServiceContract;
    }

    [HttpPut("{reviewId:guid}")]
    [Authorize(Roles = ("Admin,SuperAdmin"))]
    public async Task<IActionResult> EditReviewAsAdmin([FromBody] EditReviewAsAdminDto dto,
        [FromRoute] Guid reviewId)
    {
        var result = await _reviewServiceContract.EditReviewAsAdminAsync(dto,reviewId);
        return Ok(new
        {
            message = result
        });
    }

    [HttpDelete("{reviewId:guid}")]
    [Authorize(Roles = ("Admin,SuperAdmin"))]
    public async Task<IActionResult> DeleteReviewAsAdmin([FromRoute]Guid reviewId)
    {
        var result = await _reviewServiceContract.DeleteReviewAsAdminAsync(reviewId);
        return Ok(new
        {
            message = result
        });
    }
    
    [HttpPost("{reviewId:guid}/restore")]
    [Authorize(Roles = ("Admin,SuperAdmin"))]
    public async Task<IActionResult> RestoreReviewAsAdmin([FromRoute]Guid reviewId)
    {
        var result = await _reviewServiceContract.RestoreReviewAsAdminAsync(reviewId);
        return Ok(new
        {
            message = result
        });
    }
    [HttpGet]
    [Authorize(Roles = ("Admin,SuperAdmin"))]
    public async Task<IActionResult> GetReviews([FromQuery] ReviewStatus? status)
    {
        if (status.HasValue)
        {
            var reviews = await _reviewServiceContract.GetAllReviewsByStatusForAdmin(status.Value);
            return Ok(reviews);
        }
    
        var allReviews = await _reviewServiceContract.GetAllReviewsForAdmin();
        return Ok(allReviews);
    }

    [HttpGet("{reviewId:guid}")]
    [Authorize(Roles = ("Admin,SuperAdmin"))]
    public async Task<IActionResult> GetReviewById([FromRoute] Guid reviewId)
    {
        var review = await _reviewServiceContract.GetReviewsByIdForAdmin(reviewId);
        return Ok(review);
    }

    [HttpPatch("{reviewId:guid}/{status}")]
    [Authorize(Roles = ("Admin,SuperAdmin"))]
    public async Task<IActionResult> ChangeReviewStatus([FromRoute] Guid reviewId, [FromRoute] ReviewStatus status)
    {
        var result = await _reviewServiceContract.ChangeReviewStatus(reviewId, status);
        return Ok(new
        {
            message = result
        });
    }
    
    
}