using Domain.Enums;
using Shared.Exceptions;

namespace Domain.Entities;

public class Coupon
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = null!;
    public DiscountType DiscountType { get; private set; }
    public decimal Value { get; private set; }
    public decimal? MinimumOrderAmount { get; private set; }
    public decimal? MaxDiscountAmount { get; private set; }
    public int? UsageLimit { get; private set; }
    public int? UserUsageLimit { get; private set; }
    public int UsedCount { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public List<CouponUsage> CouponUsages { get; set; } = new();

    private Coupon()
    {
    }


    public Coupon(
        string code,
        DiscountType discountType,
        decimal value,
        DateTime startsAt,
        DateTime endAt,
        decimal? minimumOrderAmount = null,
        decimal? maxDiscountAmount = null,
        int? usageLimit = null,
        int? userUsageLimit = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessException("کد تخفیف الزامی است.");

        if (value <= 0)
            throw new BusinessException("مقدار تخفیف باید بیشتر از صفر باشد.");

        if (discountType == DiscountType.Percentage &&
            value > 100)
        {
            throw new BusinessException(
                "درصد تخفیف نمی‌تواند بیشتر از 100 باشد.");
        }

        if (discountType == DiscountType.FixedAmount &&
            maxDiscountAmount.HasValue)
        {
            throw new BusinessException(
                "برای تخفیف مبلغ ثابت نباید سقف تخفیف تعیین شود.");
        }

        if (startsAt >= endAt)
            throw new BusinessException(
                "تاریخ شروع باید قبل از تاریخ پایان باشد.");

        if (minimumOrderAmount.HasValue &&
            minimumOrderAmount <= 0)
        {
            throw new BusinessException(
                "حداقل مبلغ سفارش باید بیشتر از صفر باشد.");
        }

        if (maxDiscountAmount.HasValue &&
            maxDiscountAmount <= 0)
        {
            throw new BusinessException(
                "حداکثر مبلغ تخفیف باید بیشتر از صفر باشد.");
        }

        if (usageLimit.HasValue && usageLimit <= 0)
        {
            throw new BusinessException(
                "محدودیت استفاده باید بیشتر از صفر باشد.");
        }

        if (userUsageLimit.HasValue && userUsageLimit <= 0)
        {
            throw new BusinessException(
                "محدودیت استفاده کاربر باید بیشتر از صفر باشد.");
        }


        Id = Guid.NewGuid();

        Code = code.Trim().ToUpperInvariant();

        DiscountType = discountType;
        Value = value;

        MinimumOrderAmount = minimumOrderAmount;
        MaxDiscountAmount = maxDiscountAmount;

        UsageLimit = usageLimit;
        UserUsageLimit = userUsageLimit;

        UsedCount = 0;

        StartsAt = startsAt;
        EndAt = endAt;

        IsActive = false;
        IsDeleted = false;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void Edit(
        string? code,
        DiscountType? discountType,
        decimal? value,
        DateTime? startsAt,
        DateTime? endAt,
        decimal? minimumOrderAmount = null,
        decimal? maxDiscountAmount = null,
        int? usageLimit = null,
        int? userUsageLimit = null)
    {
        var newDiscountType = discountType ?? DiscountType;
        var newValue = value ?? Value;
        var newStartsAt = startsAt ?? StartsAt;
        var newEndAt = endAt ?? EndAt;

        if (code is not null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new BusinessException("کد تخفیف الزامی است.");

            Code = code.Trim().ToUpperInvariant();
        }

        if (newValue <= 0)
            throw new BusinessException(
                "مقدار تخفیف باید بیشتر از صفر باشد.");

        if (newDiscountType == DiscountType.Percentage &&
            newValue > 100)
        {
            throw new BusinessException(
                "درصد تخفیف نمی‌تواند بیشتر از 100 باشد.");
        }

        if (newDiscountType == DiscountType.FixedAmount &&
            maxDiscountAmount.HasValue)
        {
            throw new BusinessException(
                "برای تخفیف مبلغ ثابت نباید سقف تخفیف تعیین شود.");
        }

        if (newStartsAt >= newEndAt)
            throw new BusinessException(
                "تاریخ شروع باید قبل از تاریخ پایان باشد.");

        if (minimumOrderAmount.HasValue &&
            minimumOrderAmount.Value <= 0)
        {
            throw new BusinessException(
                "حداقل مبلغ سفارش باید بیشتر از صفر باشد.");
        }

        if (maxDiscountAmount.HasValue &&
            maxDiscountAmount.Value <= 0)
        {
            throw new BusinessException(
                "حداکثر مبلغ تخفیف باید بیشتر از صفر باشد.");
        }

        if (usageLimit.HasValue &&
            usageLimit.Value <= 0)
        {
            throw new BusinessException(
                "محدودیت استفاده باید بیشتر از صفر باشد.");
        }

        if (userUsageLimit.HasValue &&
            userUsageLimit.Value <= 0)
        {
            throw new BusinessException(
                "محدودیت استفاده کاربر باید بیشتر از صفر باشد.");
        }

        DiscountType = newDiscountType;
        Value = newValue;

        MinimumOrderAmount = minimumOrderAmount ?? MinimumOrderAmount;
        MaxDiscountAmount = maxDiscountAmount ?? MaxDiscountAmount;

        UsageLimit = usageLimit ?? UsageLimit;
        UserUsageLimit = userUsageLimit ?? UserUsageLimit;

        StartsAt = newStartsAt;
        EndAt = newEndAt;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsDeleted)
            throw new BusinessException("کد تخفیف حذف شده است .");

        if (IsActive)
            throw new BusinessException("کد تخفیف فعال است .");
        
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsDeleted)
            throw new BusinessException("کد تخفیف حذف نشده است و وجود دارد .");
        
        if (!IsActive)
            throw new BusinessException("کد تخفیف غیر فعال است .");
        
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new BusinessException("کد تخفیف ازقبل حذف شده است .");
        IsDeleted = true;
        
        var now = DateTime.UtcNow;
        UpdatedAt = now;
        DeletedAt = now;
    }
    
    public void Restore()
    {
        if (!IsDeleted)
            throw new BusinessException("کد تخفیف ازقبل موجود است .");
        IsDeleted = false;
        
        UpdatedAt = DateTime.UtcNow;
        DeletedAt = null;
    }
    
    public void IncreaseUsage()
    {
        if (UsageLimit.HasValue &&
            UsedCount >= UsageLimit.Value)
        {
            throw new BusinessException(
                "محدودیت استفاده از کد تخفیف تکمیل شده است.");
        }

        UsedCount++;
        UpdatedAt = DateTime.UtcNow;
    }
}