using Application.Features.Address.DTOs;
using FluentValidation;

namespace Application.Features.Address.Validators;

public class EditAddressValidator : AbstractValidator<EditAddressDto>
{
    public EditAddressValidator()
    {
        RuleFor(x => x.AddressTitle)
            .MaximumLength(100)
            .WithMessage("عنوان آدرس نمی‌تواند بیشتر از 100 کاراکتر باشد.")
            .When(x => x.AddressTitle is not null);

        RuleFor(x => x.ReceiverName)
            .MaximumLength(150)
            .WithMessage("نام گیرنده نمی‌تواند بیشتر از 150 کاراکتر باشد.")
            .When(x => x.ReceiverName is not null);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^09\d{9}$")
            .WithMessage("شماره تلفن معتبر نیست.")
            .When(x => x.PhoneNumber is not null);

        RuleFor(x => x.Province)
            .MaximumLength(100)
            .WithMessage("نام استان نمی‌تواند بیشتر از 100 کاراکتر باشد.")
            .When(x => x.Province is not null);

        RuleFor(x => x.City)
            .MaximumLength(100)
            .WithMessage("نام شهر نمی‌تواند بیشتر از 100 کاراکتر باشد.")
            .When(x => x.City is not null);

        RuleFor(x => x.AddressLine)
            .MaximumLength(500)
            .WithMessage("آدرس نمی‌تواند بیشتر از 500 کاراکتر باشد.")
            .When(x => x.AddressLine is not null);

        RuleFor(x => x.PostalCode)
            .Matches(@"^\d{10}$")
            .WithMessage("کد پستی باید 10 رقم باشد.")
            .When(x => x.PostalCode is not null);
        
        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrWhiteSpace(x.AddressTitle) ||
                !string.IsNullOrWhiteSpace(x.ReceiverName) ||
                !string.IsNullOrWhiteSpace(x.PhoneNumber) ||
                !string.IsNullOrWhiteSpace(x.Province) ||
                !string.IsNullOrWhiteSpace(x.City) ||
                !string.IsNullOrWhiteSpace(x.AddressLine) ||
                !string.IsNullOrWhiteSpace(x.PostalCode))
            .WithMessage("حداقل یکی از اطلاعات آدرس باید برای ویرایش ارسال شود.");
    }
}