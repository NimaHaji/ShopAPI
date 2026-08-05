using Domain.Enums;
using Shared.Exceptions;

namespace Domain.Entities;

public class Discount
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public DiscountType DiscountType { get; private set; }
    public decimal Value { get; private set; }
    public decimal? MaxDiscountAmount { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    public List<DiscountProduct> DiscountProducts { get; private set; } = new();

    public Discount(string title, DiscountType discountType, decimal value, decimal? maxDiscountAmount,
        DateTime startsAt, DateTime endsAt)
    {
        Id = Guid.NewGuid();
        Title = title;
        DiscountType = discountType;
        Value = value;
        MaxDiscountAmount = maxDiscountAmount;
        StartsAt = startsAt;
        EndsAt = endsAt;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        DeletedAt = null;
        IsDeleted = false;
    }

    public void Edit(string title, DiscountType discountType, decimal value, decimal? maxDiscountAmount,
        DateTime startsAt, DateTime endsAt)
    {
        Title = title;
        DiscountType = discountType;
        Value = value;
        MaxDiscountAmount = maxDiscountAmount;
        StartsAt = startsAt;
        EndsAt = endsAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
            throw new BusinessException("تخفیف از قبل فعال است .");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DeActivate()
    {
        if (!IsActive)
            throw new BusinessException("تخفیف از قبل غیر فعال است .");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new BusinessException("تخفیف از قبل حذف شده است .");

        IsActive = false;
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Restore()
    {
        if (!IsDeleted)
            throw new BusinessException("تخفیف وجود دارد و حذف نمی باشد .");

        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}