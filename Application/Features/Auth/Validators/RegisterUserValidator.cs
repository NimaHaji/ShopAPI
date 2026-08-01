using Application.Features.Auth.DTOs;
using FluentValidation;

namespace Application.Features.Auth.Validators;

public class RegisterUserValidator : AbstractValidator<RegisterUserRequestDto>
{
    public RegisterUserValidator()
    {

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("نام کامل نمی‌تواند خالی باشد.")
            .MinimumLength(3).WithMessage("نام کامل باید حداقل ۳ کاراکتر باشد.")
            .MaximumLength(100).WithMessage("نام کامل نباید بیش از ۱۰۰ کاراکتر باشد.")
            .Matches(@"^[\p{L}\s]+$").WithMessage("نام کامل فقط می‌تواند شامل حروف و فاصله باشد.");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("ایمیل نمی‌تواند خالی باشد.")
            .EmailAddress().WithMessage("فرمت ایمیل معتبر نیست.")
            .MaximumLength(256).WithMessage("طول ایمیل نباید بیش از ۲۵۶ کاراکتر باشد.");
        
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("شماره تلفن نمی‌تواند خالی باشد.")
            .Matches(@"^09[0-9]{9}$").WithMessage("شماره تلفن باید یک شماره موبایل معتبر ایرانی (شروع با 09 و ۱۱ رقم) باشد.");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("رمز عبور نمی‌تواند خالی باشد.")
            .MinimumLength(8).WithMessage("رمز عبور باید حداقل ۸ کاراکتر باشد.")
            .MaximumLength(50).WithMessage("طول رمز عبور نباید بیش از ۵۰ کاراکتر باشد.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._-])[A-Za-z\d@$!%*?&._-]+$")
            .WithMessage("رمز عبور باید شامل حداقل یک حرف بزرگ، یک حرف کوچک، یک عدد و یک کاراکتر خاص (@$!%*?&._-) باشد.");
    }
}