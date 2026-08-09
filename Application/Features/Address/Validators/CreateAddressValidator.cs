using Application.Features.Address.DTOs;
using FluentValidation;

namespace Application.Features.Address.Validators;

public class CreateAddressValidator : AbstractValidator<CreateAddressDto>
{
    public CreateAddressValidator()
    {
        RuleFor(x => x.AddressTitle)
            .NotEmpty()
            .WithMessage("عنوان آدرس الزامی است.")
            .MaximumLength(100)
            .WithMessage("عنوان آدرس نمی‌تواند بیشتر از 100 کاراکتر باشد.");

        RuleFor(x => x.ReceiverName)
            .NotEmpty()
            .WithMessage("نام گیرنده الزامی است.")
            .MaximumLength(150)
            .WithMessage("نام گیرنده نمی‌تواند بیشتر از 150 کاراکتر باشد.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("شماره تلفن الزامی است.")
            .Matches(@"^09\d{9}$")
            .WithMessage("شماره تلفن معتبر نیست.");

        RuleFor(x => x.Province)
            .NotEmpty()
            .WithMessage("استان الزامی است.")
            .MaximumLength(100)
            .WithMessage("نام استان نمی‌تواند بیشتر از 100 کاراکتر باشد.");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("شهر الزامی است.")
            .MaximumLength(100)
            .WithMessage("نام شهر نمی‌تواند بیشتر از 100 کاراکتر باشد.");

        RuleFor(x => x.AddressLine)
            .NotEmpty()
            .WithMessage("آدرس الزامی است.")
            .MaximumLength(500)
            .WithMessage("آدرس نمی‌تواند بیشتر از 500 کاراکتر باشد.");

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .WithMessage("کد پستی الزامی است.")
            .Matches(@"^\d{10}$")
            .WithMessage("کد پستی باید 10 رقم باشد.");
    }
}