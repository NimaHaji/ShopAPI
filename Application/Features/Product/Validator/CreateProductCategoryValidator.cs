using Application.Features.Product.DTOs;
using FluentValidation;

namespace Application.Features.Product.Validator;

public class CreateProductCategoryValidator:AbstractValidator<CreateProductCategoryDto>
{
    public CreateProductCategoryValidator()
    {
        RuleFor(pc => pc.Title)
            .NotEmpty()
            .WithMessage("نام دسته بندی محصول الزامی است.")
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("نام دسته بندی محصول نمی‌تواند خالی یا فقط شامل فاصله باشد.")
            .Must(title => !title.All(char.IsDigit))
            .WithMessage("نام دسته بندی محصول نمی‌تواند فقط عدد باشد.")
            .MaximumLength(200)
            .WithMessage("نام دسته بندی محصول نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد.");
    }
}