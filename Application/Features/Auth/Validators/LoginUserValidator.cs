using Application.Features.Auth.DTOs;
using FluentValidation;

namespace Application.Features.Auth.Validators;

public class LoginUserValidator:AbstractValidator<LoginUserRequestDto>
{
    public LoginUserValidator()
    {
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("ایمیل نمی‌تواند خالی باشد.")
            .EmailAddress().WithMessage("فرمت ایمیل معتبر نیست.")
            .MaximumLength(256).WithMessage("طول ایمیل نباید بیش از ۲۵۶ کاراکتر باشد.");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("رمز عبور نمی‌تواند خالی باشد.")
            .MinimumLength(6).WithMessage("رمز عبور باید حداقل ۶ کاراکتر باشد.")
            .MaximumLength(50).WithMessage("طول رمز عبور نباید بیش از ۵۰ کاراکتر باشد.");
    }
}