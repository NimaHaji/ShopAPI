using Application.Features.Coupon.DTOs;
using Domain.Enums;
using FluentValidation;

namespace Application.Features.Coupon.Validators;

public class CreateCouponValidator : AbstractValidator<CreateCouponDto>
{
    public CreateCouponValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("کد تخفیف الزامی است.");

        RuleFor(x => x.Code)
            .MaximumLength(50)
            .WithMessage("کد تخفیف نباید بیشتر از 50 کاراکتر باشد.");

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .WithMessage("مقدار تخفیف باید بیشتر از صفر باشد.");

        RuleFor(x => x)
            .Must(x =>
            {
                if (x.DiscountType == DiscountType.Percentage)
                    return x.Value <= 100;

                return true;
            })
            .WithMessage("درصد تخفیف نمی‌تواند بیشتر از 100 باشد.");

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinimumOrderAmount.HasValue)
            .WithMessage("حداقل مبلغ سفارش نمی‌تواند منفی باشد.");

        RuleFor(x => x.MaxDiscountAmount)
            .GreaterThan(0)
            .When(x => x.MaxDiscountAmount.HasValue)
            .WithMessage("حداکثر مبلغ تخفیف باید بیشتر از صفر باشد.");

        RuleFor(x => x)
            .Must(x =>
            {
                if (x.DiscountType == DiscountType.FixedAmount)
                    return !x.MaxDiscountAmount.HasValue;

                return true;
            })
            .WithMessage("برای تخفیف مبلغ ثابت نباید سقف تخفیف تعیین شود.");

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0)
            .When(x => x.UsageLimit.HasValue)
            .WithMessage("محدودیت استفاده باید بیشتر از صفر باشد.");

        RuleFor(x => x.UserUsageLimit)
            .GreaterThan(0)
            .When(x => x.UserUsageLimit.HasValue)
            .WithMessage("محدودیت استفاده کاربر باید بیشتر از صفر باشد.");

        RuleFor(x => x)
            .Must(x => x.StartsAt < x.EndAt)
            .WithMessage("تاریخ شروع باید قبل از تاریخ پایان باشد.");
        
        RuleFor(x => x.DiscountType)
            .IsInEnum()
            .WithMessage("نوع تخفیف نامعتبر است.");
    }
}