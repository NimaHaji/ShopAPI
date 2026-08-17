using Application.Features.Discount.DTOs;
using Domain.Enums;
using FluentValidation;

namespace Application.Features.Discount.Validators;

public class EditDiscountValidator : AbstractValidator<EditDiscountDto>
{
    public EditDiscountValidator()
    {

        RuleFor(x => x)
            .Must(HaveAtLeastOneChange)
            .WithMessage("حداقل یک مقدار برای ویرایش باید ارسال شود.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .When(x => x.Title is not null)
            .WithMessage("عنوان تخفیف نمی‌تواند خالی باشد.");

        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => x.Title is not null)
            .WithMessage("عنوان تخفیف نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد.");

        RuleFor(x => x.DiscountType)
            .IsInEnum()
            .When(x => x.DiscountType.HasValue)
            .WithMessage("نوع تخفیف نامعتبر است.");

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .When(x => x.Value.HasValue)
            .WithMessage("مقدار تخفیف باید بیشتر از صفر باشد.");

        RuleFor(x => x.Value)
            .InclusiveBetween(1, 100)
            .When(x =>
                x.Value.HasValue &&
                x.DiscountType == DiscountType.Percentage)
            .WithMessage("درصد تخفیف باید بین ۱ تا ۱۰۰ باشد.");

        RuleFor(x => x.MaxDiscountAmount)
            .GreaterThan(0)
            .When(x => x.MaxDiscountAmount.HasValue)
            .WithMessage("حداکثر مبلغ تخفیف باید بیشتر از صفر باشد.");

        RuleFor(x => x)
            .Must(HaveValidDateRange)
            .WithMessage("تاریخ شروع تخفیف باید قبل از تاریخ پایان باشد.");
    }

    private static bool HaveAtLeastOneChange(EditDiscountDto dto)
    {
        return dto.Title is not null ||
               dto.DiscountType.HasValue ||
               dto.Value.HasValue ||
               dto.MaxDiscountAmount.HasValue ||
               dto.StartsAt.HasValue ||
               dto.EndsAt.HasValue;
    }

    private static bool HaveValidDateRange(EditDiscountDto dto)
    {
        if (!dto.StartsAt.HasValue || !dto.EndsAt.HasValue)
            return true;

        return dto.StartsAt.Value < dto.EndsAt.Value;
    }
}