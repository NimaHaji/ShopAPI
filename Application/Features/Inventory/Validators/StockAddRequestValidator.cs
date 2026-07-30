using Application.Features.Inventory.DTOs;
using FluentValidation;

namespace Application.Features.Inventory.Validators;

public class StockAddRequestValidator : AbstractValidator<StockAddRequestDto>
{
    public StockAddRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("شناسه محصول نمی‌تواند خالی باشد.")
            .Must(id => id != Guid.Empty).WithMessage("شناسه محصول معتبر نیست.");
        
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("تعداد باید بزرگتر از صفر باشد.")
            .LessThanOrEqualTo(1000).WithMessage("تعداد وارد شده بسیار زیاد است (حداکثر ۱۰۰۰ عدد مجاز است).");
        
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("توضیحات نباید بیش از ۵۰۰ کاراکتر باشد.");
    }
}