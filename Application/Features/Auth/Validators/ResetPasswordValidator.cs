using Application.Features.Auth.DTOs;
using FluentValidation;

namespace Application.Features.Auth.Validators;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordValidator()
    {

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("ایمیل نمی‌تواند خالی باشد.")
            .EmailAddress().WithMessage("فرمت ایمیل معتبر نیست.")
            .MaximumLength(256).WithMessage("طول ایمیل نباید بیش از ۲۵۶ کاراکتر باشد.");
        
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("کد تایید نمی‌تواند خالی باشد.")
            .Matches(@"^\d{6}$").WithMessage("کد تایید باید دقیقاً ۶ رقم باشد.");
        
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("رمز عبور جدید نمی‌تواند خالی باشد.")
            .MinimumLength(8).WithMessage("رمز عبور باید حداقل ۸ کاراکتر باشد.")
            .MaximumLength(50).WithMessage("طول رمز عبور نباید بیش از ۵۰ کاراکتر باشد.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._-])[A-Za-z\d@$!%*?&._-]+$")
            .WithMessage("رمز عبور باید شامل حداقل یک حرف بزرگ، یک حرف کوچک، یک عدد و یک کاراکتر خاص (@$!%*?&._-) باشد.");
    }
}