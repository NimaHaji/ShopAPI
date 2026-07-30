using Application.Features.Product.DTOs;
using FluentValidation;

namespace Application.Features.Product.Validator;

public class CreateBrandValidator:AbstractValidator<CreateProductBrandDto>
{
    public CreateBrandValidator()
    {
        RuleFor(pc => pc.Title)
            .NotEmpty()
            .WithMessage("نام برند محصول الزامی است.")
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("نام برند محصول نمی‌تواند خالی یا فقط شامل فاصله باشد.")
            .Must(title => !title.All(char.IsDigit))
            .WithMessage("نام برند محصول نمی‌تواند فقط عدد باشد.")
            .MaximumLength(200)
            .WithMessage("نام برند محصول نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد.");
    }
}