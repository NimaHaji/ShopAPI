using Application.Features.Discount.DTOs;
using Domain.Enums;
using FluentValidation;

namespace Application.Features.Discount.Validators;

public class CreateDiscountValidator : AbstractValidator<CreateDiscountDto>
{
    public CreateDiscountValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("عنوان تخفیف الزامی است.")
            .MaximumLength(100)
            .WithMessage("عنوان تخفیف نمی‌تواند بیشتر از 100 کاراکتر باشد.");

        RuleFor(x => x.DiscountType)
            .IsInEnum()
            .WithMessage("نوع تخفیف نامعتبر است.");

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .WithMessage("مقدار تخفیف باید بیشتر از صفر باشد.");

        RuleFor(x => x.MaxDiscountAmount)
            .GreaterThan(0)
            .When(x => x.MaxDiscountAmount.HasValue)
            .WithMessage("حداکثر مبلغ تخفیف باید بیشتر از صفر باشد.");

        RuleFor(x => x.StartsAt)
            .LessThan(x => x.EndsAt)
            .WithMessage("تاریخ شروع تخفیف باید قبل از تاریخ پایان باشد.");

        RuleFor(x => x.EndsAt)
            .GreaterThan(x => x.StartsAt)
            .WithMessage("تاریخ پایان تخفیف باید بعد از تاریخ شروع باشد.");

        RuleFor(x => x)
            .Must(HaveValidPercentage)
            .WithMessage("درصد تخفیف باید بین ۱ تا ۱۰۰ باشد.")
            .When(x => x.DiscountType == DiscountType.Percentage);
    }

    private static bool HaveValidPercentage(CreateDiscountDto dto)
    {
        return dto.Value >= 1 && dto.Value <= 100;
    }
}