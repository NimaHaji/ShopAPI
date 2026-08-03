using Domain.Enums;
using Shared.Exceptions;

namespace Domain.Entities;

public class Review
{
    public Guid Id { get; set; }
    public int StarsCount { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    public Guid ProductId { get; set; }
    public Product Product { get; set; }

    public ReviewStatus ReviewStatus { get; set; }
    private Review()
    {
        
    }

    public Review(int starsCount,string comment, Guid userId, Guid productId)
    {
        if (starsCount < 1 || starsCount > 5)
            throw new BusinessException("امتیاز باید بین 1 تا 5 باشد.");
        
        Id = Guid.NewGuid();
        StarsCount = starsCount;
        Comment = comment;
        UserId = userId;
        ProductId = productId;
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
        IsDeleted = false;
        ReviewStatus = ReviewStatus.Pending;
    }

    public void Edit(string? comment, int? starsCount)
    {
        if (starsCount.HasValue && (starsCount < 1 || starsCount > 5))
            throw new BusinessException("امتیاز باید بین 1 تا 5 باشد.");

        if (comment is not null)
            Comment = comment;

        if (starsCount.HasValue)
            StarsCount = starsCount.Value;

        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
    }

    public void ChangeStatus(ReviewStatus status)
    {
        ReviewStatus = status;
        UpdatedAt = DateTime.Now;
    }

    public void Restore()
    {
        if (!IsDeleted)
            throw new BusinessException("این نظر حذف نشده است .");
        
        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTime.Now;
    }
}