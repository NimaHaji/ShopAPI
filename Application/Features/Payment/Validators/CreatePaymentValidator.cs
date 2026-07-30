using Application.Features.Payment.DTOs;
using FluentValidation;

namespace Application.Features.Payment.Validators;

public class CreatePaymentValidator : AbstractValidator<CreatePaymentDto>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("شناسه سفارش نمی‌تواند خالی باشد.")
            .Must(id => id != Guid.Empty).WithMessage("شناسه سفارش معتبر نیست.");
        
        RuleFor(x => x.Gateway)
            .IsInEnum().WithMessage("درگاه پرداخت انتخاب شده معتبر نیست.");
        
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("توضیحات نمی‌تواند خالی باشد.")
            .MaximumLength(500).WithMessage("توضیحات نباید بیش از ۵۰۰ کاراکتر باشد.");
        
        RuleFor(x => x.Mobile)
            .Matches(@"^09[0-9]{9}$").WithMessage("شماره موبایل باید یک شماره معتبر ایرانی (شروع با 09 و ۱۱ رقم) باشد.")
            .When(x => !string.IsNullOrEmpty(x.Mobile));
        
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("فرمت ایمیل معتبر نیست.")
            .MaximumLength(256).WithMessage("طول ایمیل نباید بیش از ۲۵۶ کاراکتر باشد.")
            .When(x => !string.IsNullOrEmpty(x.Email));
        
        RuleFor(x => x.GatewayFee)
            .GreaterThanOrEqualTo(0).WithMessage("کارمزد درگاه نمی‌تواند منفی باشد.")
            .When(x => x.GatewayFee.HasValue);
        
        RuleFor(x => x.TokenExpiryInMinutes)
            .GreaterThan(0).WithMessage("مدت اعتبار توکن باید بزرگتر از صفر باشد.")
            .LessThanOrEqualTo(1440).WithMessage("مدت اعتبار توکن نمی‌تواند بیش از ۱۴۴۰ دقیقه (یک روز) باشد.")
            .When(x => x.TokenExpiryInMinutes.HasValue);
        
        RuleFor(x => x.ReferrerId)
            .MaximumLength(100).WithMessage("شناسه ارجاع‌دهنده نباید بیش از ۱۰۰ کاراکتر باشد.")
            .When(x => !string.IsNullOrEmpty(x.ReferrerId));
        
        RuleFor(x => x)
            .Must(dto => !string.IsNullOrEmpty(dto.Mobile) || !string.IsNullOrEmpty(dto.Email))
            .WithMessage("حداقل یکی از شماره موبایل یا ایمیل باید وارد شود.")
            .When(x => string.IsNullOrEmpty(x.Mobile) && string.IsNullOrEmpty(x.Email));
    }
}