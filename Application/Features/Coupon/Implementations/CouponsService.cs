using Application.Common.Interfaces;
using Application.Features.Cart.Interfaces;
using Application.Features.Coupon.DTOs;
using Application.Features.Coupon.Interfaces;
using Domain.Enums;
using Shared.Exceptions;

namespace Application.Features.Coupon.Implementations;

public class CouponService : CouponsServiceContract
{
    private readonly CouponRepositoryContract _couponRepositoryContract;
    private readonly UnitOfWorkContract _unitOfWorkContract;
    private readonly IUSerContext _userContext;
    private readonly CartRepositoryContract _cartRepositoryContract;

    public CouponService(CouponRepositoryContract couponRepositoryContract, UnitOfWorkContract unitOfWorkContract,
        IUSerContext userContext, CartRepositoryContract cartRepositoryContract)
    {
        _couponRepositoryContract = couponRepositoryContract;
        _unitOfWorkContract = unitOfWorkContract;
        _userContext = userContext;
        _cartRepositoryContract = cartRepositoryContract;
    }

    public async Task<ViewCouponDto> GetAllCouponsForAdminAsync()
    {
        var coupons = await _couponRepositoryContract.GetAllDiscountsForAdminAsync();
        return new ViewCouponDto
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
    }

    public async Task<ViewCouponitemsDto?> GetCouponByIdForAdminAsync(Guid couponId)
    {
        var coupon = await _couponRepositoryContract.GetCouponByIdForAdminAsync(couponId);

        if (coupon is null)
            throw new NotFoundException("کد تخفیف یافت نشد .");

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
        var isCouponExist = await _couponRepositoryContract.IsCouponCodeExistAsync(dto.Code);

        if (isCouponExist)
            throw new BusinessException("این کد تخفیف قبلا وجود دارد .");

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

        return "کد تخفیف با موفقیت ساخته شد .";
    }

    public async Task<string> EditCouponAsync(EditCouponDto dto)
    {
        var coupon = await _couponRepositoryContract.GetCouponByIdForAdminAsync(dto.Id);

        if (coupon is null)
            throw new NotFoundException("کد تخفیف یافت نشد .");

        coupon.Edit(
            code: dto.Code,
            discountType: dto.DiscountType,
            value: dto.Value,
            startsAt: dto.StartsAt,
            endAt: dto.StartsAt,
            minimumOrderAmount: dto.MinimumOrderAmount,
            maxDiscountAmount: dto.MaxDiscountAmount,
            usageLimit: dto.UsageLimit,
            userUsageLimit: dto.UserUsageLimit
        );

        await _unitOfWorkContract.SaveAsync();
        return "کد تخفیف با موفقیت تغییر پیدا کرد .";
    }

    public async Task<string> DeleteCouponAsync(Guid couponId)
    {
        var coupon = await _couponRepositoryContract.GetCouponByIdForAdminAsync(couponId);

        if (coupon is null)
            throw new NotFoundException("کد تخفیف یافت نشد .");

        coupon.Delete();

        await _unitOfWorkContract.SaveAsync();
        return "کد تخفیف با موفقیت حذف شد .";
    }

    public async Task<string> RestoreCouponAsync(Guid couponId)
    {
        var coupon = await _couponRepositoryContract.GetCouponByIdForAdminAsync(couponId);

        if (coupon is null)
            throw new NotFoundException("کد تخفیف یافت نشد .");

        coupon.Restore();

        await _unitOfWorkContract.SaveAsync();
        return "کد تخفیف با موفقیت بازیابی شد .";
    }

    public async Task<string> ActivateCouponAsync(Guid couponId)
    {
        var coupon = await _couponRepositoryContract.GetCouponByIdForAdminAsync(couponId);

        if (coupon is null)
            throw new NotFoundException("کد تخفیف یافت نشد .");

        coupon.Activate();

        await _unitOfWorkContract.SaveAsync();
        return "کد تخفیف با موفقیت فعال شد .";
    }

    public async Task<string> DeActivateCouponAsync(Guid couponId)
    {
        var coupon = await _couponRepositoryContract.GetCouponByIdForAdminAsync(couponId);

        if (coupon is null)
            throw new NotFoundException("کد تخفیف یافت نشد .");

        coupon.Deactivate();

        await _unitOfWorkContract.SaveAsync();
        return "کد تخفیف با موفقیت غیر فعال شد .";
    }

    public async Task<ValidateCouponResponseDto> ValidateCouponAsync(
        ValidateCouponDto dto)
    {
        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        var code = dto.Code.Trim().ToUpperInvariant();

        var coupon = await _couponRepositoryContract
            .GetCouponByCodeAsync(code);

        if (coupon is null)
            throw new NotFoundException("کد تخفیف یافت نشد.");

        if (!coupon.IsActive)
            throw new BusinessException("کد تخفیف فعال نیست.");

        var now = DateTime.UtcNow;

        if (now < coupon.StartsAt)
            throw new BusinessException(
                "زمان استفاده از این کد تخفیف هنوز شروع نشده است.");

        if (now > coupon.EndAt)
            throw new BusinessException(
                "اعتبار این کد تخفیف به پایان رسیده است.");

        if (coupon.UsageLimit.HasValue &&
            coupon.UsedCount >= coupon.UsageLimit.Value)
        {
            throw new BusinessException(
                "محدودیت استفاده از این کد تخفیف به پایان رسیده است.");
        }

        var userUsageCount = await _couponRepositoryContract
            .GetUserCouponUsageCountAsync(
                coupon.Id,
                userId);

        if (coupon.UserUsageLimit.HasValue &&
            userUsageCount >= coupon.UserUsageLimit.Value)
        {
            throw new BusinessException(
                "نمی‌توانید بیش از حد مجاز از این کد تخفیف استفاده کنید.");
        }

        var cart = await _cartRepositoryContract
            .GetCartWithProductsByUserIdAsync(userId);

        if (cart is null || !cart.CartItems.Any())
            throw new NotFoundException(
                "سبد خریدی برای کاربر یافت نشد.");

        // مبلغ کالاها بعد از تخفیف Product/Variant
        long cartTotalPrice = 0;

        foreach (var item in cart.CartItems)
        {
            var variant = item.ProductVariant;

            if (variant is null)
                throw new NotFoundException(
                    "Variant محصول یافت نشد.");

            if (variant.IsDeleted)
                throw new BusinessException(
                    "یکی از Variantهای سبد خرید دیگر قابل خرید نیست.");

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

            // Variant Discount اولویت دارد
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

        if (coupon.MinimumOrderAmount.HasValue &&
            cartTotalPrice < coupon.MinimumOrderAmount.Value)
        {
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