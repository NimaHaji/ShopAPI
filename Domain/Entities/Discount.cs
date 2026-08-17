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

    public List<DiscountVariant> DiscountVariants { get; private set; } = new();
    public List<DiscountProduct> DiscountProducts { get; private set; } = new();
    private Discount()
    {
    }

    public Discount(
        string title,
        DiscountType discountType,
        decimal value,
        decimal? maxDiscountAmount,
        DateTime startsAt,
        DateTime endsAt)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessException("عنوان تخفیف الزامی است.");

        if (startsAt >= endsAt)
            throw new BusinessException(
                "تاریخ شروع تخفیف باید قبل از تاریخ پایان باشد.");

        if (value <= 0)
            throw new BusinessException(
                "مقدار تخفیف باید بیشتر از صفر باشد.");

        if (maxDiscountAmount is <= 0)
            throw new BusinessException(
                "حداکثر مبلغ تخفیف باید بیشتر از صفر باشد.");

        Id = Guid.NewGuid();

        Title = title.Trim();

        DiscountType = discountType;

        Value = value;

        MaxDiscountAmount = maxDiscountAmount;

        StartsAt = startsAt;

        EndsAt = endsAt;

        IsActive = true;

        CreatedAt = DateTime.UtcNow;

        UpdatedAt = DateTime.UtcNow;

        IsDeleted = false;
    }

    public void Edit(
        string? title,
        DiscountType? discountType,
        decimal? value,
        decimal? maxDiscountAmount,
        DateTime? startsAt,
        DateTime? endsAt)
    {
        var newStartsAt = startsAt ?? StartsAt;
        var newEndsAt = endsAt ?? EndsAt;

        if (newStartsAt >= newEndsAt)
            throw new BusinessException(
                "تاریخ شروع تخفیف باید قبل از تاریخ پایان باشد.");

        if (value is <= 0)
            throw new BusinessException(
                "مقدار تخفیف باید بیشتر از صفر باشد.");

        if (maxDiscountAmount is <= 0)
            throw new BusinessException(
                "حداکثر مبلغ تخفیف باید بیشتر از صفر باشد.");

        if (title is not null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new BusinessException(
                    "عنوان تخفیف نمی‌تواند خالی باشد.");

            Title = title.Trim();
        }

        if (discountType.HasValue)
            DiscountType = discountType.Value;

        if (value.HasValue)
            Value = value.Value;

        if (maxDiscountAmount.HasValue)
            MaxDiscountAmount = maxDiscountAmount.Value;

        if (startsAt.HasValue)
            StartsAt = startsAt.Value;

        if (endsAt.HasValue)
            EndsAt = endsAt.Value;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsDeleted)
            throw new BusinessException(
                "تخفیف حذف شده را نمی‌توان فعال کرد.");

        if (IsActive)
            throw new BusinessException(
                "تخفیف از قبل فعال است.");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DeActivate()
    {
        if (!IsActive)
            throw new BusinessException(
                "تخفیف از قبل غیرفعال است.");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new BusinessException(
                "تخفیف از قبل حذف شده است.");

        IsDeleted = true;
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        if (!IsDeleted)
            throw new BusinessException(
                "تخفیف حذف نشده است.");

        IsDeleted = false;
        DeletedAt = null;

        UpdatedAt = DateTime.UtcNow;
    }
}