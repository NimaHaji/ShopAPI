using Application.Features.Discount.DTOs;
using Domain.Enums;
using FluentValidation;

namespace Application.Features.Discount.Validators;

public class EditDiscountValidator : AbstractValidator<EditDiscountDto>
{
    public EditDiscountValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("عنوان تخفیف الزامی است.")
            .MaximumLength(200)
            .WithMessage("عنوان تخفیف نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد.");

        RuleFor(x => x.DiscountType)
            .IsInEnum()
            .WithMessage("نوع تخفیف نامعتبر است.");

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .WithMessage("مقدار تخفیف باید بیشتر از صفر باشد.");

        RuleFor(x => x.Value)
            .InclusiveBetween(1, 100)
            .When(x => x.DiscountType == DiscountType.Percentage)
            .WithMessage("درصد تخفیف باید بین ۱ تا ۱۰۰ باشد.");

        RuleFor(x => x.MaxDiscountAmount)
            .GreaterThan(0)
            .When(x => x.MaxDiscountAmount.HasValue)
            .WithMessage("حداکثر مبلغ تخفیف باید بیشتر از صفر باشد.");

        RuleFor(x => x.StartsAt)
            .LessThan(x => x.EndsAt)
            .WithMessage("تاریخ شروع تخفیف باید قبل از تاریخ پایان باشد.");
    }
}