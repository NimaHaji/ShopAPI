using Application.Features.Cart.DTOs;
using FluentValidation;

namespace Application.Features.Cart.Validators;

public class UpdateCartItemValidator : AbstractValidator<AddCartItemDto>
{
    public UpdateCartItemValidator()
    {
        
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("شناسه محصول نمی‌تواند خالی باشد.")
            .Must(id => id != Guid.Empty).WithMessage("شناسه محصول معتبر نیست.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("تعداد باید بزرگتر از صفر باشد.");
    }
}