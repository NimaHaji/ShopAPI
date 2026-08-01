using Application.Features.Product.DTOs;
using FluentValidation;

namespace Application.Features.Product.Validators;

public class EditProductBrandValidator:AbstractValidator<EditProductBrandDto>
{
    public EditProductBrandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("شناسه عددی برند محصول نمیتواند خالی باشد .");
        
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان نمی تواند خالی باشد .")
            .MaximumLength(200).WithMessage("عنوان نمی تواند بیشتر از 200 کاراکتر باشد .");
    }
}