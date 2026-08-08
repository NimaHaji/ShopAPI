using Application.Features.Coupon.DTOs;
using Domain.Enums;
using FluentValidation;

namespace Application.Features.Coupon.Validators;

public class EditCouponValidator : AbstractValidator<EditCouponDto>
{
    public EditCouponValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("شناسه Coupon الزامی است.");

        RuleFor(x => x.Code)
            .MaximumLength(50)
            .When(x => x.Code != null)
            .WithMessage("کد تخفیف نمی‌تواند بیشتر از 50 کاراکتر باشد.");

        RuleFor(x => x.DiscountType)
            .IsInEnum()
            .When(x => x.DiscountType.HasValue)
            .WithMessage("نوع تخفیف نامعتبر است.");

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .When(x => x.Value.HasValue)
            .WithMessage("مقدار تخفیف باید بیشتر از صفر باشد.");

        RuleFor(x => x)
            .Must(x =>
            {
                if (x.DiscountType == DiscountType.Percentage &&
                    x.Value.HasValue)
                {
                    return x.Value <= 100;
                }

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

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0)
            .When(x => x.UsageLimit.HasValue)
            .WithMessage("محدودیت استفاده باید بیشتر از صفر باشد.");

        RuleFor(x => x.UserUsageLimit)
            .GreaterThan(0)
            .When(x => x.UserUsageLimit.HasValue)
            .WithMessage("محدودیت استفاده کاربر باید بیشتر از صفر باشد.");

        RuleFor(x => x)
            .Must(x =>
            {
                if (x.StartsAt.HasValue && x.EndAt.HasValue)
                    return x.StartsAt < x.EndAt;

                return true;
            })
            .WithMessage("تاریخ شروع باید قبل از تاریخ پایان باشد.");
        
        RuleFor(x => x)
            .Must(HasAtLeastOneChange)
            .WithMessage("حداقل یک مقدار باید برای تغییر ارسال شود.");
    }


    private bool HasAtLeastOneChange(EditCouponDto dto)
    {
        return dto.Code != null ||
               dto.DiscountType.HasValue ||
               dto.Value.HasValue ||
               dto.MinimumOrderAmount.HasValue ||
               dto.MaxDiscountAmount.HasValue ||
               dto.UsageLimit.HasValue ||
               dto.UserUsageLimit.HasValue ||
               dto.StartsAt.HasValue ||
               dto.EndAt.HasValue;
    }
}