using Application.Features.Discount.DTOs;
using FluentValidation;

namespace Application.Features.Discount.Validators;

public class AddProductToDiscountValidator : AbstractValidator<AddProductToDiscountDto>
{
    public AddProductToDiscountValidator()
    {
        RuleFor(x => x.ProductIds)
            .NotEmpty()
            .WithMessage("لیست محصولات الزامی است.");

        RuleForEach(x => x.ProductIds)
            .NotEmpty()
            .WithMessage("شناسه محصول الزامی است.")
            .NotEqual(Guid.Empty)
            .WithMessage("شناسه محصول نامعتبر است.");
    }
}