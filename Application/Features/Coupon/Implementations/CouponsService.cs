using Application.Common.Interfaces;
using Application.Common.Observability;
using Application.Features.Cart.Interfaces;
using Application.Features.Coupon.DTOs;
using Application.Features.Coupon.Interfaces;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Exceptions;

namespace Application.Features.Coupon.Implementations;

public class CouponService : CouponsServiceContract
{
    private readonly CouponRepositoryContract _couponRepositoryContract;
    private readonly UnitOfWorkContract _unitOfWorkContract;
    private readonly IUSerContext _userContext;
    private readonly CartRepositoryContract _cartRepositoryContract;
    private readonly ILogger<CouponService> _logger;

    public CouponService(CouponRepositoryContract couponRepositoryContract, UnitOfWorkContract unitOfWorkContract,
        IUSerContext userContext, CartRepositoryContract cartRepositoryContract, ILogger<CouponService> logger)
    {
        _couponRepositoryContract = couponRepositoryContract;
        _unitOfWorkContract = unitOfWorkContract;
        _userContext = userContext;
        _cartRepositoryContract = cartRepositoryContract;
        _logger = logger;
    }

    public async Task<ViewCouponDto> GetAllCouponsForAdminAsync()
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(GetAllCouponsForAdminAsync));

        _logger.LogInformation(
            LogEvents.Coupon.GetAllStarted,
            "Getting all coupons for admin.");

        var coupons =
            await _couponRepositoryContract
                .GetAllDiscountsForAdminAsync();

        var result = new ViewCouponDto
        {
            CouponItems = coupons.Select(c => new ViewCouponitemsDto
            {
                Id = c.Id,
                Code = c.Code,
                DiscountType = c.DiscountType.ToString(),
                Value = c.Value,
                MinimumOrderAmount = c.MinimumOrderAmount,
                MaxDiscountAmount = c.MaxDiscountAmount,
                UsageLimit = c.UsageLimit,
                UserUsageLimit = c.UserUsageLimit,
                UsedCount = c.UsedCount,
                StartsAt = c.StartsAt,
                EndAt = c.EndAt,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                DeletedAt = c.DeletedAt,
                IsDeleted = c.IsDeleted
            }).ToList()
        };

        activity?.SetTag(
            "coupons.count",
            result.CouponItems.Count);

        _logger.LogInformation(
            LogEvents.Coupon.GetAllCompleted,
            "All coupons retrieved successfully. Count: {Count}",
            result.CouponItems.Count);

        return result;
    }

    public async Task<ViewCouponitemsDto?> GetCouponByIdForAdminAsync(
        Guid couponId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(GetCouponByIdForAdminAsync));

        activity?.SetTag("coupon.id", couponId);

        var coupon =
            await _couponRepositoryContract
                .GetCouponByIdForAdminAsync(couponId);

        if (coupon is null)
        {
            _logger.LogWarning(
                LogEvents.Coupon.NotFound,
                "Coupon was not found. CouponId: {CouponId}",
                couponId);

            throw new NotFoundException(
                "کد تخفیف یافت نشد .");
        }

        _logger.LogInformation(
            LogEvents.Coupon.GetByIdCompleted,
            "Coupon retrieved successfully. CouponId: {CouponId}",
            couponId);

        return new ViewCouponitemsDto
        {
            Id = coupon.Id,
            Code = coupon.Code,
            DiscountType = coupon.DiscountType.ToString(),
            Value = coupon.Value,
            MinimumOrderAmount = coupon.MinimumOrderAmount,
            MaxDiscountAmount = coupon.MaxDiscountAmount,
            UsageLimit = coupon.UsageLimit,
            UserUsageLimit = coupon.UserUsageLimit,
            UsedCount = coupon.UsedCount,
            StartsAt = coupon.StartsAt,
            EndAt = coupon.EndAt,
            IsActive = coupon.IsActive,
            CreatedAt = coupon.CreatedAt,
            UpdatedAt = coupon.UpdatedAt,
            DeletedAt = coupon.DeletedAt,
            IsDeleted = coupon.IsDeleted
        };
    }

    public async Task<string> CreateCouponAsync(CreateCouponDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(CreateCouponAsync));

        _logger.LogInformation(
            LogEvents.Coupon.CreateStarted,
            "Creating coupon.");

        var isCouponExist =
            await _couponRepositoryContract
                .IsCouponCodeExistAsync(dto.Code);

        if (isCouponExist)
        {
            _logger.LogWarning(
                LogEvents.Coupon.Duplicate,
                "Coupon creation rejected because the coupon code already exists.");

            throw new BusinessException(
                "این کد تخفیف قبلا وجود دارد .");
        }

        var coupon = new Domain.Entities.Coupon(
            code: dto.Code,
            discountType: dto.DiscountType,
            value: dto.Value,
            startsAt: dto.StartsAt,
            endAt: dto.EndAt,
            minimumOrderAmount: dto.MinimumOrderAmount,
            maxDiscountAmount: dto.MaxDiscountAmount,
            usageLimit: dto.UsageLimit,
            userUsageLimit: dto.UserUsageLimit
        );

        await _couponRepositoryContract.CreatCouponAsync(coupon);
        await _unitOfWorkContract.SaveAsync();

        activity?.SetTag("coupon.id", coupon.Id);

        _logger.LogInformation(
            LogEvents.Coupon.CreateCompleted,
            "Coupon created successfully. CouponId: {CouponId}",
            coupon.Id);

        return "کد تخفیف با موفقیت ساخته شد .";
    }

    public async Task<string> EditCouponAsync(EditCouponDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(EditCouponAsync));

        activity?.SetTag("coupon.id", dto.Id);

        _logger.LogInformation(
            LogEvents.Coupon.EditStarted,
            "Editing coupon. CouponId: {CouponId}",
            dto.Id);

        var coupon =
            await _couponRepositoryContract
                .GetCouponByIdForAdminAsync(dto.Id);

        if (coupon is null)
        {
            _logger.LogWarning(
                LogEvents.Coupon.NotFound,
                "Coupon edit rejected because the coupon was not found. CouponId: {CouponId}",
                dto.Id);

            throw new NotFoundException(
                "کد تخفیف یافت نشد .");
        }

        coupon.Edit(
            code: dto.Code,
            discountType: dto.DiscountType,
            value: dto.Value,
            startsAt: dto.StartsAt,
            endAt: dto.EndAt,
            minimumOrderAmount: dto.MinimumOrderAmount,
            maxDiscountAmount: dto.MaxDiscountAmount,
            usageLimit: dto.UsageLimit,
            userUsageLimit: dto.UserUsageLimit
        );

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Coupon.EditCompleted,
            "Coupon edited successfully. CouponId: {CouponId}",
            dto.Id);

        return "کد تخفیف با موفقیت تغییر پیدا کرد .";
    }

    public async Task<string> DeleteCouponAsync(Guid couponId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(DeleteCouponAsync));

        activity?.SetTag("coupon.id", couponId);

        _logger.LogInformation(
            LogEvents.Coupon.DeleteStarted,
            "Deleting coupon. CouponId: {CouponId}",
            couponId);

        var coupon =
            await _couponRepositoryContract
                .GetCouponByIdForAdminAsync(couponId);

        if (coupon is null)
        {
            _logger.LogWarning(
                LogEvents.Coupon.NotFound,
                "Coupon deletion rejected because the coupon was not found. CouponId: {CouponId}",
                couponId);

            throw new NotFoundException(
                "کد تخفیف یافت نشد .");
        }

        coupon.Delete();

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Coupon.DeleteCompleted,
            "Coupon deleted successfully. CouponId: {CouponId}",
            couponId);

        return "کد تخفیف با موفقیت حذف شد .";
    }

    public async Task<string> RestoreCouponAsync(Guid couponId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(RestoreCouponAsync));

        activity?.SetTag("coupon.id", couponId);

        _logger.LogInformation(
            LogEvents.Coupon.RestoreStarted,
            "Restoring coupon. CouponId: {CouponId}",
            couponId);

        var coupon =
            await _couponRepositoryContract
                .GetCouponByIdForAdminAsync(couponId);

        if (coupon is null)
        {
            _logger.LogWarning(
                LogEvents.Coupon.NotFound,
                "Coupon restore rejected because the coupon was not found. CouponId: {CouponId}",
                couponId);

            throw new NotFoundException(
                "کد تخفیف یافت نشد .");
        }

        coupon.Restore();

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Coupon.RestoreCompleted,
            "Coupon restored successfully. CouponId: {CouponId}",
            couponId);

        return "کد تخفیف با موفقیت بازیابی شد .";
    }

    public async Task<string> ActivateCouponAsync(Guid couponId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(ActivateCouponAsync));

        activity?.SetTag("coupon.id", couponId);

        _logger.LogInformation(
            LogEvents.Coupon.ActivateStarted,
            "Activating coupon. CouponId: {CouponId}",
            couponId);

        var coupon =
            await _couponRepositoryContract
                .GetCouponByIdForAdminAsync(couponId);

        if (coupon is null)
        {
            _logger.LogWarning(
                LogEvents.Coupon.NotFound,
                "Coupon activation rejected because the coupon was not found. CouponId: {CouponId}",
                couponId);

            throw new NotFoundException(
                "کد تخفیف یافت نشد .");
        }

        coupon.Activate();

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Coupon.ActivateCompleted,
            "Coupon activated successfully. CouponId: {CouponId}",
            couponId);

        return "کد تخفیف با موفقیت فعال شد .";
    }

    public async Task<string> DeActivateCouponAsync(Guid couponId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(DeActivateCouponAsync));

        activity?.SetTag("coupon.id", couponId);

        _logger.LogInformation(
            LogEvents.Coupon.DeactivateStarted,
            "Deactivating coupon. CouponId: {CouponId}",
            couponId);

        var coupon =
            await _couponRepositoryContract
                .GetCouponByIdForAdminAsync(couponId);

        if (coupon is null)
        {
            _logger.LogWarning(
                LogEvents.Coupon.NotFound,
                "Coupon deactivation rejected because the coupon was not found. CouponId: {CouponId}",
                couponId);

            throw new NotFoundException(
                "کد تخفیف یافت نشد .");
        }

        coupon.Deactivate();

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Coupon.DeactivateCompleted,
            "Coupon deactivated successfully. CouponId: {CouponId}",
            couponId);

        return "کد تخفیف با موفقیت غیر فعال شد .";
    }
    
    public async Task<ValidateCouponResponseDto> ValidateCouponAsync(
    ValidateCouponDto dto)
{
    using var activity =
        ActivitySources.ShopApi.StartActivity(nameof(ValidateCouponAsync));

    var userId = _userContext.UserId
                 ?? throw new UnauthorizedAccessException(
                     "کاربر احراز هویت نشده است.");

    activity?.SetTag("user.id", userId);

    _logger.LogInformation(
        LogEvents.Coupon.ValidateStarted,
        "Coupon validation started.");

    var code = dto.Code.Trim().ToUpperInvariant();

    var coupon =
        await _couponRepositoryContract
            .GetCouponByCodeAsync(code);

    if (coupon is null)
    {
        _logger.LogWarning(
            LogEvents.Coupon.ValidationRejected,
            "Coupon validation rejected because the coupon was not found.");

        throw new NotFoundException(
            "کد تخفیف یافت نشد.");
    }

    activity?.SetTag("coupon.id", coupon.Id);

    if (!coupon.IsActive)
    {
        _logger.LogWarning(
            LogEvents.Coupon.ValidationRejected,
            "Coupon validation rejected because the coupon is inactive. CouponId: {CouponId}",
            coupon.Id);

        throw new BusinessException(
            "کد تخفیف فعال نیست.");
    }

    var now = DateTime.UtcNow;

    if (now < coupon.StartsAt)
    {
        _logger.LogWarning(
            LogEvents.Coupon.ValidationRejected,
            "Coupon validation rejected because the coupon has not started yet. CouponId: {CouponId}",
            coupon.Id);

        throw new BusinessException(
            "زمان استفاده از این کد تخفیف هنوز شروع نشده است.");
    }

    if (now > coupon.EndAt)
    {
        _logger.LogWarning(
            LogEvents.Coupon.ValidationRejected,
            "Coupon validation rejected because the coupon has expired. CouponId: {CouponId}",
            coupon.Id);

        throw new BusinessException(
            "اعتبار این کد تخفیف به پایان رسیده است.");
    }

    if (coupon.UsageLimit.HasValue &&
        coupon.UsedCount >= coupon.UsageLimit.Value)
    {
        _logger.LogWarning(
            LogEvents.Coupon.ValidationRejected,
            "Coupon validation rejected because the usage limit has been reached. CouponId: {CouponId}",
            coupon.Id);

        throw new BusinessException(
            "محدودیت استفاده از این کد تخفیف به پایان رسیده است.");
    }

    var userUsageCount =
        await _couponRepositoryContract
            .GetUserCouponUsageCountAsync(
                coupon.Id,
                userId);

    if (coupon.UserUsageLimit.HasValue &&
        userUsageCount >= coupon.UserUsageLimit.Value)
    {
        _logger.LogWarning(
            LogEvents.Coupon.ValidationRejected,
            "Coupon validation rejected because the user usage limit has been reached. CouponId: {CouponId}",
            coupon.Id);

        throw new BusinessException(
            "نمی‌توانید بیش از حد مجاز از این کد تخفیف استفاده کنید.");
    }

    var cart =
        await _cartRepositoryContract
            .GetCartWithProductsByUserIdAsync(userId);

    if (cart is null || !cart.CartItems.Any())
    {
        _logger.LogWarning(
            LogEvents.Coupon.ValidationRejected,
            "Coupon validation rejected because the cart is empty or not found. CouponId: {CouponId}",
            coupon.Id);

        throw new NotFoundException(
            "سبد خریدی برای کاربر یافت نشد.");
    }

    long cartTotalPrice = 0;

    foreach (var item in cart.CartItems)
    {
        var variant = item.ProductVariant;

        if (variant is null)
        {
            _logger.LogWarning(
                LogEvents.Coupon.ValidationRejected,
                "Coupon validation rejected because a product variant was not found. CouponId: {CouponId}",
                coupon.Id);

            throw new NotFoundException(
                "Variant محصول یافت نشد.");
        }

        if (variant.IsDeleted)
        {
            _logger.LogWarning(
                LogEvents.Coupon.ValidationRejected,
                "Coupon validation rejected because a cart product variant is deleted. CouponId: {CouponId}",
                coupon.Id);

            throw new BusinessException(
                "یکی از Variantهای سبد خرید دیگر قابل خرید نیست.");
        }

        var unitPrice = variant.Price;

        var variantDiscount = variant.DiscountVariants
            .Select(dv => dv.Discount)
            .FirstOrDefault(d =>
                !d.IsDeleted &&
                d.IsActive &&
                d.StartsAt <= now &&
                d.EndsAt > now);

        var productDiscount = variant.Product
            .DiscountProducts
            .Select(dp => dp.Discount)
            .FirstOrDefault(d =>
                !d.IsDeleted &&
                d.IsActive &&
                d.StartsAt <= now &&
                d.EndsAt > now);

        var activeDiscount =
            variantDiscount ?? productDiscount;

        long discountAmount = 0;

        if (activeDiscount is not null)
        {
            if (activeDiscount.DiscountType ==
                DiscountType.Percentage)
            {
                discountAmount = (long)(
                    unitPrice *
                    activeDiscount.Value /
                    100);

                if (activeDiscount.MaxDiscountAmount.HasValue)
                {
                    discountAmount = Math.Min(
                        discountAmount,
                        (long)activeDiscount.MaxDiscountAmount.Value);
                }
            }
            else if (activeDiscount.DiscountType ==
                     DiscountType.FixedAmount)
            {
                discountAmount = Math.Min(
                    (long)activeDiscount.Value,
                    unitPrice);
            }
        }

        var finalUnitPrice =
            Math.Max(0, unitPrice - discountAmount);

        cartTotalPrice +=
            finalUnitPrice * item.Quantity;
    }

    activity?.SetTag(
        "coupon.cart_total_price",
        cartTotalPrice);

    if (coupon.MinimumOrderAmount.HasValue &&
        cartTotalPrice < coupon.MinimumOrderAmount.Value)
    {
        _logger.LogWarning(
            LogEvents.Coupon.ValidationRejected,
            "Coupon validation rejected because the cart total is below the minimum order amount. CouponId: {CouponId}",
            coupon.Id);

        throw new BusinessException(
            $"حداقل مبلغ سفارش برای استفاده از این کد تخفیف " +
            $"{coupon.MinimumOrderAmount.Value:N0} است.");
    }

    long discountAmountByCoupon;

    if (coupon.DiscountType == DiscountType.Percentage)
    {
        discountAmountByCoupon = (long)(
            cartTotalPrice *
            coupon.Value /
            100);
    }
    else if (coupon.DiscountType ==
             DiscountType.FixedAmount)
    {
        discountAmountByCoupon =
            (long)coupon.Value;
    }
    else
    {
        _logger.LogWarning(
            LogEvents.Coupon.ValidationRejected,
            "Coupon validation rejected because the discount type is invalid. CouponId: {CouponId}",
            coupon.Id);

        throw new BusinessException(
            "نوع تخفیف نامعتبر است.");
    }

    if (coupon.MaxDiscountAmount.HasValue)
    {
        discountAmountByCoupon = Math.Min(
            discountAmountByCoupon,
            (long)coupon.MaxDiscountAmount.Value);
    }

    discountAmountByCoupon = Math.Min(
        discountAmountByCoupon,
        cartTotalPrice);

    var finalPrice =
        cartTotalPrice - discountAmountByCoupon;

    activity?.SetTag(
        "coupon.discount_amount",
        discountAmountByCoupon);

    activity?.SetTag(
        "coupon.final_price",
        finalPrice);

    _logger.LogInformation(
        LogEvents.Coupon.ValidateCompleted,
        "Coupon validated successfully. CouponId: {CouponId}, DiscountAmount: {DiscountAmount}",
        coupon.Id,
        discountAmountByCoupon);

    return new ValidateCouponResponseDto
    {
        CouponId = coupon.Id,
        Code = coupon.Code,
        DiscountAmount = discountAmountByCoupon,
        CartTotalPrice = cartTotalPrice,
        FinalPrice = finalPrice
    };
}
}