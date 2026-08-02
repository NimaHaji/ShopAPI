using Application.Features.Review.DTOs;
using FluentValidation;

namespace Application.Features.Review.Validators;

public class CreateReviewValidator : AbstractValidator<CreateReviewDto>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.Comment)
            .NotEmpty()
            .WithMessage("متن نظر الزامی است.")
            .MaximumLength(1000)
            .WithMessage("متن نظر نمی‌تواند بیشتر از 1000 کاراکتر باشد.");

        RuleFor(x => x.StarsCount)
            .InclusiveBetween(1, 5)
            .WithMessage("امتیاز باید بین 1 تا 5 باشد.");
    }
}