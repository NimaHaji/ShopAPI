using Application.Features.Auth.DTOs;
using FluentValidation;

namespace Application.Features.Auth.Validators;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileRequestDto>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("نام کامل نمی‌تواند خالی باشد.")
            .MinimumLength(3).WithMessage("نام کامل باید حداقل ۳ کاراکتر باشد.")
            .MaximumLength(100).WithMessage("نام کامل نباید بیش از ۱۰۰ کاراکتر باشد.")
            .Matches(@"^[\p{L}\s]+$").WithMessage("نام کامل فقط می‌تواند شامل حروف و فاصله باشد.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("شماره تلفن نمی‌تواند خالی باشد.")
            .Matches(@"^09[0-9]{9}$").WithMessage("شماره تلفن باید یک شماره موبایل معتبر ایرانی (شروع با 09 و ۱۱ رقم) باشد.");
    }
}