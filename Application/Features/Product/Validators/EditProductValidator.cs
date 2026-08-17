using Application.Features.Product.DTOs;
using FluentValidation;

namespace Application.Features.Product.Validators;

public class EditProductValidator : AbstractValidator<EditProductDto>
{
    public EditProductValidator()
    {
        RuleFor(x => x)
            .Must(x =>
                x.Title is not null ||
                x.Description is not null)
            .WithMessage(
                "حداقل یکی از فیلدهای عنوان یا توضیحات باید برای ویرایش ارسال شود.");
            
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("شناسه عددی محصول نمیتواند خالی باشد .");
        
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان نمی تواند خالی باشد .")
            .MaximumLength(200).WithMessage("عنوان نمی تواند بیشتر از 200 کاراکتر باشد .")
            .When(x => x.Title != null);
        
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("توضیحات نمی تواند خالی باشد .")
            .MaximumLength(2000).WithMessage("توضیحات نمی تواند بیشتر از 2000 کاراکتر باشد .")
            .When(x => x.Description != null);
        
        
    }
}