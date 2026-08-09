using Application.Features.Checkout.DTOs;
using FluentValidation;

namespace Application.Features.Checkout.Validator;

public class CheckoutValidator : AbstractValidator<CheckoutDto>
{
    public CheckoutValidator()
    {
        RuleFor(x => x.CouponCode)
            .MaximumLength(50)
            .WithMessage("کد تخفیف نمی‌تواند بیشتر از 50 کاراکتر باشد.")
            .When(x => x.CouponCode != null);
    }
}