using Application.Features.Inventory.DTOs;
using FluentValidation;

namespace Application.Features.Inventory.Validators;

public class StockReserveRequestValidator : AbstractValidator<StockReserveRequestDto>
{
    public StockReserveRequestValidator()
    {
        
        RuleFor(x => x.ProductVariantId)
            .NotEmpty().WithMessage("شناسه محصول نمی‌تواند خالی باشد.")
            .Must(id => id != Guid.Empty).WithMessage("شناسه محصول معتبر نیست.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("تعداد درخواستی باید بزرگتر از صفر باشد.");
        
        RuleFor(x => x.OrderReference)
            .MaximumLength(50).WithMessage("مرجع سفارش نباید بیش از ۵۰ کاراکتر باشد.")
            .When(x => !string.IsNullOrEmpty(x.OrderReference));
    }
}