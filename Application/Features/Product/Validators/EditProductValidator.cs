using Application.Features.Product.DTOs;
using FluentValidation;

namespace Application.Features.Product.Validators;

public class EditProductValidator : AbstractValidator<EditProductDto>
{
    public EditProductValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("شناسه عددی محصول نمیتواند خالی باشد .");
        
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان نمی تواند خالی باشد .")
            .MaximumLength(200).WithMessage("عنوان نمی تواند بیشتر از 200 کاراکتر باشد .")
            .When(x => x.Title != null);
        
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("توضیحات نمی تواند خالی باشد .")
            .MaximumLength(2000).WithMessage("توضیحات نمی تواند بیشتر از 2000 کاراکتر باشد .")
            .When(x => x.Description != null);
        
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("قیمت محصول نمی تواند منفی باشد .")
            .When(x => x.Price.HasValue);
        
        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100).WithMessage("درصد تخفیف باید بین 0 تا 100 باشد .")
            .When(x => x.DiscountPercentage.HasValue);
    }
}