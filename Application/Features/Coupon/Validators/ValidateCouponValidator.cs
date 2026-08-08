using Application.Features.Coupon.DTOs;
using FluentValidation;

namespace Application.Features.Coupon.Validators;

public class ValidateCouponValidator : AbstractValidator<ValidateCouponDto>
{
    public ValidateCouponValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("کد تخفیف الزامی است.");

        RuleFor(x => x.Code)
            .MaximumLength(50)
            .WithMessage("کد تخفیف نمی‌تواند بیشتر از 50 کاراکتر باشد.");
    }
}