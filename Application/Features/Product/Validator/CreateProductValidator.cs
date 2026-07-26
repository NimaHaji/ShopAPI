using Application.Features.Product.DTOs;
using FluentValidation;

namespace Application.Features.Product.Validator;

public class CreateProductValidator:AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(p => p.Title)
            .NotEmpty()
            .WithMessage("نام محصول الزامی است.")
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("نام محصول نمی‌تواند خالی یا فقط شامل فاصله باشد.")
            .Must(title => !title.All(char.IsDigit))
            .WithMessage("نام محصول نمی‌تواند فقط عدد باشد.")
            .MaximumLength(200)
            .WithMessage("نام محصول نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد.");
        
        RuleFor(p => p.Description)
            .MaximumLength(2000)
            .WithMessage("توضیحات نمی‌تواند بیشتر از 2000 کاراکتر باشد.");

        RuleFor(p => p.Price)
            .GreaterThan(0)
            .WithMessage("قیمت محصول باید بزرگ‌تر از صفر باشد.");

        RuleFor(p => p.Stock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("موجودی نمی‌تواند منفی باشد.");

        RuleFor(p => p.CategoryId)
            .NotEmpty()
            .WithMessage("دسته‌بندی الزامی است.");

        RuleFor(p => p.BrandId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("شناسه برند معتبر نیست.");
    }
}