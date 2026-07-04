using Application.Features.Auth.DTOs;
using FluentValidation;

namespace Application.Validator.User;

public class RegisterUserValidator : AbstractValidator<RegisterUserRequestDto>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.FullName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("نام کامل الزامی است .")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("نام کامل نباید خالی یا فاصله باشد .")
            .MaximumLength(50)
            .WithMessage("نام کامل نمیتواند بیشتر از 50 کاراکتر باشد");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("ایمیل الزامی است .")
            .EmailAddress().WithMessage("ایمیل معتیر نیست .")
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("پسورد الزامی است")
            .MinimumLength(8)
            .WithMessage("پسوورد باید حداقل 8 کاراکتر داشته باشد .")
            .Matches("[A-Z]")
            .WithMessage("پسوورد باید حداقل شامل یک حرف بزرگ باشد .")
            .Matches("[a-z]")
            .WithMessage("پسوورد باید حداقل یک حرف کوچک داشته باشد .")
            .Matches("[0-9]")
            .WithMessage("پسوورد باید حداقل یک عدد داشته باشد .");

        RuleFor(x => x.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("شماره موبایل الزامی است .")
            .Matches(@"^09\d{9}$")
            .WithMessage("شماره موبایل باید معتبر باشد .");
    }
}