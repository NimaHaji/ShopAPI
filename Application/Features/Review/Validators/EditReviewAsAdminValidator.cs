using Application.Features.Review.DTOs;
using FluentValidation;

namespace Application.Features.Review.Validators;

public class EditReviewAsAdminValidator : AbstractValidator<EditReviewAsAdminDto>
{
    public EditReviewAsAdminValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Comment is not null || x.StarCount is not null)
            .WithMessage("حداقل یکی از فیلدهای Comment یا StarCount باید ارسال شود.");

        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .WithMessage("متن نظر نمی‌تواند بیشتر از 1000 کاراکتر باشد.");

        RuleFor(x => x.StarCount)
            .InclusiveBetween(1, 5)
            .When(x => x.StarCount.HasValue)
            .WithMessage("امتیاز باید بین 1 تا 5 باشد.");
    }
}