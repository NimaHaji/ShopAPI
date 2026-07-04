using Application.Features.Auth.DTOs;
using FluentValidation;

namespace Application.Validator.User;

public class UpdateProfileValidator:AbstractValidator<UpdateProfileRequestDto>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FullName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("نام کامل الزامی است.")
            .MinimumLength(3)
            .WithMessage("نام کامل باید حداقل ۳ کاراکتر باشد.")
            .MaximumLength(100)
            .WithMessage("نام کامل نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.");

        RuleFor(x => x.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("شماره تلفن الزامی است.")
            .Matches(@"^09\d{9}$")
            .WithMessage("فرمت شماره تلفن معتبر نیست (مثال: 09123456789).");
    }   
}