using FluentValidation;
using Application.Features.Product.DTOs;

namespace Application.Features.Product.Validators;

public class EditProductVariantValidator
    : AbstractValidator<EditProductVariantDto>
{
    public EditProductVariantValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("شناسه Variant الزامی است.");

        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrWhiteSpace(x.Sku) ||
                x.Price.HasValue)
            .WithMessage(
                "حداقل یکی از SKU یا قیمت باید برای ویرایش ارسال شود.");

        RuleFor(x => x.Sku)
            .MaximumLength(100)
            .WithMessage("SKU نمی‌تواند بیشتر از 100 کاراکتر باشد.")
            .When(x => x.Sku is not null);

        RuleFor(x => x.Sku)
            .NotEmpty()
            .WithMessage("SKU نمی‌تواند خالی باشد.")
            .When(x => x.Sku is not null);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("قیمت نمی‌تواند منفی باشد.")
            .When(x => x.Price.HasValue);
    }
}