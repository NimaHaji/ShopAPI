using Application.Features.Wishlist.DTOs;
using FluentValidation;

namespace Application.Features.Wishlist.Validator;

public class AddWishlistItemValidator:AbstractValidator<AddWishlistItemDto>
{
    public AddWishlistItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("شناسه محصول الزامی است .")
            .NotEqual(Guid.Empty).WithMessage("فرمت شناسه محصول اشتباه است .");
    }
}