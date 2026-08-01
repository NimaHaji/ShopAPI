using Application.Features.Product.DTOs;
using FluentValidation;

namespace Application.Features.Product.Validators;

public class ProductQueryDtoValidator : AbstractValidator<ProductQueryDto>
{
    public ProductQueryDtoValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("شماره صفحه باید بزرگ‌تر از صفر باشد.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("تعداد آیتم‌های صفحه باید بین 1 تا 100 باشد.");

        RuleFor(x => x.Q)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Q))
            .WithMessage("عبارت جستجو نمی‌تواند بیشتر از 100 کاراکتر باشد.");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue)
            .WithMessage("حداقل قیمت نمی‌تواند منفی باشد.");

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxPrice.HasValue)
            .WithMessage("حداکثر قیمت نمی‌تواند منفی باشد.");

        RuleFor(x => x)
            .Must(x =>
                !x.MinPrice.HasValue ||
                !x.MaxPrice.HasValue ||
                x.MinPrice <= x.MaxPrice)
            .WithMessage("حداقل قیمت نمی‌تواند بیشتر از حداکثر قیمت باشد.");
    }
}