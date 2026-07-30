using Application.Features.Auth.DTOs;
using FluentValidation;

namespace Application.Features.Auth.Validators;

public class ForgotPasswordValidator:AbstractValidator<ForgetPasswordRequestDto>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("ایمیل نمی‌تواند خالی باشد.")
            .EmailAddress().WithMessage("فرمت ایمیل معتبر نیست.")
            .MaximumLength(256).WithMessage("طول ایمیل نباید بیش از ۲۵۶ کاراکتر باشد.");
    }
}