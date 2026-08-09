using Application.Common.Interfaces;
using Application.Features.Discount.DTOs;
using Application.Features.Discount.Interfaces;
using Application.Features.DiscountProduct.Interfaces;
using Application.Features.Product.Interfaces;
using Shared.Exceptions;

namespace Application.Features.Discount.Implementations;

public class DiscountService : DiscountServiceContract
{
    private readonly DiscountRepositoryContract _discountRepositoryContract;
    private readonly DiscountProductRepositoryContract _discountProductRepositoryContract;
    private readonly ProductRepositoryContract _productRepositoryContract;
    private readonly UnitOfWorkContract _unitOfWorkContract;

    public DiscountService(DiscountRepositoryContract discountRepositoryContract, UnitOfWorkContract unitOfWorkContract,
        ProductRepositoryContract productRepositoryContract,
        DiscountProductRepositoryContract discountProductRepositoryContract)
    {
        _discountRepositoryContract = discountRepositoryContract;
        _unitOfWorkContract = unitOfWorkContract;
        _productRepositoryContract = productRepositoryContract;
        _discountProductRepositoryContract = discountProductRepositoryContract;
    }

    public async Task<ViewDiscountDto> GetAllDiscountsAsync()
    {
        var discounts = await _discountRepositoryContract.GetAllActiveDiscountAsync();

        if (discounts is null)
            return new ViewDiscountDto
            {
                DiscountItems = []
            };

        return new ViewDiscountDto
        {
            DiscountItems = discounts.Select(x => new ViewDiscountItemsDto
            {
                Id = x.Id,
                Title = x.Title,
                DiscountType = x.DiscountType.ToString(),
                Value = x.Value,
                MaxDiscountAmount = x.MaxDiscountAmount,
                StartsAt = x.StartsAt,
                EndsAt = x.EndsAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToList()
        };
    }

    public async Task<ViewDiscountItemsDto> GetDiscountByIdAsync(Guid discountId)
    {
        var discount = await _discountRepositoryContract.GetActiveDiscountByIdAsync(discountId);

        if (discount is null)
            throw new NotFoundException("تخفیف یافت نشد .");

        return new ViewDiscountItemsDto
        {
            Id = discount.Id,
            Title = discount.Title,
            DiscountType = discount.DiscountType.ToString(),
            Value = discount.Value,
            MaxDiscountAmount = discount.MaxDiscountAmount,
            StartsAt = discount.StartsAt,
            EndsAt = discount.EndsAt,
            IsActive = discount.IsActive,
            CreatedAt = discount.CreatedAt,
            UpdatedAt = discount.UpdatedAt
        };
    }

    public async Task<string> ActivateDiscountAsync(Guid discountId)
    {
        var discount = await _discountRepositoryContract.GetDiscountByIdAsync(discountId);

        if (discount is null)
            throw new NotFoundException("تخفیف یافت نشد .");

        discount.Activate();

        await _unitOfWorkContract.SaveAsync();
        return "تخفیف با موفقیت فعال شد";
    }

    public async Task<string> DeActivateDiscountAsync(Guid discountId)
    {
        var discount = await _discountRepositoryContract.GetDiscountByIdAsync(discountId);

        if (discount is null)
            throw new NotFoundException("تخفیف یافت نشد .");

        discount.DeActivate();

        await _unitOfWorkContract.SaveAsync();
        return "تخفیف با موفقیت غیر فعال شد";
    }

    public async Task<string> CreateDiscountAsync(CreateDiscountDto dto)
    {
        var discount = new Domain.Entities.Discount(
            title: dto.Title,
            discountType: dto.DiscountType,
            value: dto.Value,
            maxDiscountAmount: dto.MaxDiscountAmount,
            startsAt: dto.StartsAt,
            endsAt: dto.EndsAt
        );

        await _discountRepositoryContract.CreateDiscountAsync(discount);
        await _unitOfWorkContract.SaveAsync();

        return "تخفیف با موفقیت ایجاد شد .";
    }

    public async Task<string> EditDiscountByIdAsync(Guid discountId, EditDiscountDto dto)
    {
        if (discountId == Guid.Empty)
            throw new BusinessException("شناسه تخفیف نامعتبر است .");

        var discount = await _discountRepositoryContract.GetDiscountByIdAsync(discountId);

        if (discount is null)
            throw new NotFoundException("تخفیف یافت نشد .");

        discount.Edit(
            title: dto.Title,
            discountType: dto.DiscountType,
            value: dto.Value,
            maxDiscountAmount: dto.MaxDiscountAmount,
            startsAt: dto.StartsAt,
            endsAt: dto.EndsAt
        );

        await _unitOfWorkContract.SaveAsync();
        return "تخفیف با موفقیت تغییر یافت .";
    }

    public async Task<string> DeleteDiscountByIdAsync(Guid discountId)
    {
        if (discountId == Guid.Empty)
            throw new BusinessException("شناسه تخفیف معتبر نیست .");

        var discount = await _discountRepositoryContract.GetDiscountByIdAsync(discountId);

        if (discount is null)
            throw new NotFoundException("تخفیف یافت نشد .");

        discount.Delete();

        await _unitOfWorkContract.SaveAsync();
        return "تخفیف با موفقیت حذف شد .";
    }

    public async Task<string> RestoreDiscountByIdAsync(Guid discountId)
    {
        if (discountId == Guid.Empty)
            throw new BusinessException("شناسه تخفیف معتبر نیست .");

        var discount = await _discountRepositoryContract.GetDiscountForAdminByIdAsync(discountId);

        if (discount is null)
            throw new NotFoundException("تخفیف یافت نشد .");

        discount.Restore();

        await _unitOfWorkContract.SaveAsync();
        return "تخفیف با موفقیت بازیابی شد .";
    }

    public async Task<string> SetDiscountForProductAsync(Guid discountId, AddProductToDiscountDto dto)
    {
        
        if (discountId == Guid.Empty)
            throw new BusinessException("شناسه تخفیف نامعتبر است.");

        if (dto.ProductIds is null || !dto.ProductIds.Any())
            throw new BusinessException("حداقل یک محصول باید انتخاب شود.");

        var discount = await _discountRepositoryContract
            .GetDiscountByIdAsync(discountId);

        if (discount is null)
            throw new NotFoundException("تخفیف یافت نشد.");

        var products = await _productRepositoryContract
            .GetProductsByIdsAsync(dto.ProductIds);

        if (products is null || !products.Any())
            throw new NotFoundException("محصولی یافت نشد.");
        
        var existingProductIds =
            await _discountProductRepositoryContract
                .GetExistingDiscountProductsAsync(discountId, dto.ProductIds);
        
        if (existingProductIds.Any())
        {
            throw new BusinessException(
                "برخی محصولات قبلا این تخفیف را دارند."
            );
        }
        
        await _unitOfWorkContract.BeginTransactionAsync();
        try
        {
            foreach (var product in products)
            {
                var discountProduct = new Domain.Entities.DiscountProduct(
                    discountId,
                    product.Id
                );

                await _discountProductRepositoryContract
                    .AddProductToDiscountAsync(discountProduct);
            }
            await _unitOfWorkContract.SaveAsync();
            await _unitOfWorkContract.CommitTransactionAsync();
        }
        catch 
        {
            await _unitOfWorkContract.RollbackTransactionAsync();
             _unitOfWorkContract.ClearChangeTracker();
             
             throw;
        }
        
        return "تخفیف با موفقیت به محصولات اضافه شد.";
    }

    public async Task<string> DeleteDiscountFoProduct(Guid discountId, Guid productId)
    {
        if (discountId == Guid.Empty)
            throw new BusinessException("شناسه تخفیف نامعتبر است.");
        
        if (productId == Guid.Empty)
            throw new BusinessException("شناسه محصول نامعتبر است.");
        
        var discount = await _discountRepositoryContract
            .GetDiscountByIdAsync(discountId);

        if (discount is null)
            throw new NotFoundException("تخفیف یافت نشد.");

        var product = await _productRepositoryContract.GetProductByIdAsync(productId);

        if (product is null)
            throw new NotFoundException("محصول یافت نشد .");
        
        var discountProduct =
            await _discountProductRepositoryContract
                .GetDiscountProductAsync(discountId, productId);

        if (discountProduct is null)
            throw new NotFoundException(
                "این تخفیف برای محصول موردنظر وجود ندارد.");

        await _discountProductRepositoryContract
            .RemoveAsync(discountProduct);

        await _unitOfWorkContract.SaveAsync();
        return "تخفیف با موفقیت از محصول برداشته شد .";
    }

    public async Task<ViewDiscountItemsDto> GetDiscountByProductId(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new BusinessException("شناسه محصول نامعتبر است .");

        var discount = await _discountProductRepositoryContract.GetDiscountByProductIdAsync(productId);

        if (discount is null)
            throw new NotFoundException("تخفیفی برای محصول پیدا نشد .");
        
        return new ViewDiscountItemsDto
        {
            Id = discount.Id,
            Title = discount.Title,
            DiscountType = discount.DiscountType.ToString(),
            Value = discount.Value,
            MaxDiscountAmount = discount.MaxDiscountAmount,
            StartsAt = discount.StartsAt,
            EndsAt = discount.EndsAt,
            IsActive = discount.IsActive,
            CreatedAt = discount.CreatedAt,
            UpdatedAt = discount.UpdatedAt
        };
    }
}