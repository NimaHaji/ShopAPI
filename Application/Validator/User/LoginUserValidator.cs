using Application.Features.Auth.DTOs;
using FluentValidation;

namespace Application.Validator.User;

public class LoginUserValidator:AbstractValidator<LoginUserRequestDto>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("ایمیل نا معتبر است .");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("پسوورد الزامی است .");
    }
}