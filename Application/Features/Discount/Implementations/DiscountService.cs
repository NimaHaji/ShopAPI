using Application.Common.Interfaces;
using Application.Common.Observability;
using Application.Features.Discount.DTOs;
using Application.Features.Discount.Interfaces;
using Application.Features.DiscountProduct.Interfaces;
using Application.Features.DiscountVariant.Interfaces;
using Application.Features.Product.Interfaces;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Exceptions;

namespace Application.Features.Discount.Implementations;

public class DiscountService : DiscountServiceContract
{
    private readonly DiscountRepositoryContract _discountRepositoryContract;
    private readonly DiscountProductRepositoryContract _discountProductRepositoryContract;
    private readonly DiscountVariantRepositoryContract _discountVariantRepositoryContract;
    private readonly ProductRepositoryContract _productRepositoryContract;
    private readonly UnitOfWorkContract _unitOfWorkContract;
    private readonly ILogger<DiscountService> _logger;

    public DiscountService(DiscountRepositoryContract discountRepositoryContract, UnitOfWorkContract unitOfWorkContract,
        ProductRepositoryContract productRepositoryContract,
        DiscountProductRepositoryContract discountProductRepositoryContract,
        DiscountVariantRepositoryContract discountVariantRepositoryContract, ILogger<DiscountService> logger)
    {
        _discountRepositoryContract = discountRepositoryContract;
        _unitOfWorkContract = unitOfWorkContract;
        _productRepositoryContract = productRepositoryContract;
        _discountProductRepositoryContract = discountProductRepositoryContract;
        _discountVariantRepositoryContract = discountVariantRepositoryContract;
        _logger = logger;
    }

    public async Task<ViewDiscountDto> GetAllDiscountsAsync()
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(GetAllDiscountsAsync));

        _logger.LogInformation(
            LogEvents.Discount.GetAllStarted,
            "Getting all active discounts.");

        var discounts =
            await _discountRepositoryContract
                .GetAllActiveDiscountAsync();

        if (discounts is null)
        {
            activity?.SetTag("discounts.count", 0);

            _logger.LogInformation(
                LogEvents.Discount.GetAllCompleted,
                "No active discounts found.");

            return new ViewDiscountDto
            {
                DiscountItems = []
            };
        }

        var result = new ViewDiscountDto
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

        activity?.SetTag(
            "discounts.count",
            result.DiscountItems.Count);

        _logger.LogInformation(
            LogEvents.Discount.GetAllCompleted,
            "Active discounts retrieved successfully. Count: {Count}",
            result.DiscountItems.Count);

        return result;
    }

    public async Task<ViewDiscountItemsDto> GetDiscountByIdAsync(
        Guid discountId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(GetDiscountByIdAsync));

        activity?.SetTag("discount.id", discountId);

        var discount =
            await _discountRepositoryContract
                .GetActiveDiscountByIdAsync(discountId);

        if (discount is null)
        {
            _logger.LogWarning(
                LogEvents.Discount.NotFound,
                "Discount was not found. DiscountId: {DiscountId}",
                discountId);

            throw new NotFoundException(
                "تخفیف یافت نشد .");
        }

        _logger.LogInformation(
            LogEvents.Discount.GetByIdCompleted,
            "Discount retrieved successfully. DiscountId: {DiscountId}",
            discountId);

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
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(ActivateDiscountAsync));

        activity?.SetTag("discount.id", discountId);

        _logger.LogInformation(
            LogEvents.Discount.ActivateStarted,
            "Activating discount. DiscountId: {DiscountId}",
            discountId);

        var discount =
            await _discountRepositoryContract
                .GetDiscountByIdAsync(discountId);

        if (discount is null)
        {
            _logger.LogWarning(
                LogEvents.Discount.NotFound,
                "Discount activation rejected because the discount was not found. DiscountId: {DiscountId}",
                discountId);

            throw new NotFoundException(
                "تخفیف یافت نشد .");
        }

        discount.Activate();

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Discount.ActivateCompleted,
            "Discount activated successfully. DiscountId: {DiscountId}",
            discountId);

        return "تخفیف با موفقیت فعال شد";
    }

    public async Task<string> DeActivateDiscountAsync(Guid discountId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(DeActivateDiscountAsync));

        activity?.SetTag("discount.id", discountId);

        _logger.LogInformation(
            LogEvents.Discount.DeactivateStarted,
            "Deactivating discount. DiscountId: {DiscountId}",
            discountId);

        var discount =
            await _discountRepositoryContract
                .GetDiscountByIdAsync(discountId);

        if (discount is null)
        {
            _logger.LogWarning(
                LogEvents.Discount.NotFound,
                "Discount deactivation rejected because the discount was not found. DiscountId: {DiscountId}",
                discountId);

            throw new NotFoundException(
                "تخفیف یافت نشد .");
        }

        discount.DeActivate();

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Discount.DeactivateCompleted,
            "Discount deactivated successfully. DiscountId: {DiscountId}",
            discountId);

        return "تخفیف با موفقیت غیر فعال شد";
    }

    public async Task<string> CreateDiscountAsync(CreateDiscountDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(CreateDiscountAsync));

        _logger.LogInformation(
            LogEvents.Discount.CreateStarted,
            "Creating discount.");

        var discount = new Domain.Entities.Discount(
            title: dto.Title,
            discountType: dto.DiscountType,
            value: dto.Value,
            maxDiscountAmount: dto.MaxDiscountAmount,
            startsAt: dto.StartsAt,
            endsAt: dto.EndsAt
        );

        await _discountRepositoryContract
            .CreateDiscountAsync(discount);

        await _unitOfWorkContract.SaveAsync();

        activity?.SetTag("discount.id", discount.Id);

        _logger.LogInformation(
            LogEvents.Discount.CreateCompleted,
            "Discount created successfully. DiscountId: {DiscountId}",
            discount.Id);

        return "تخفیف با موفقیت ایجاد شد .";
    }

    public async Task<string> EditDiscountByIdAsync(
        Guid discountId,
        EditDiscountDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(EditDiscountByIdAsync));

        activity?.SetTag("discount.id", discountId);

        if (discountId == Guid.Empty)
            throw new BusinessException(
                "شناسه تخفیف نامعتبر است .");

        _logger.LogInformation(
            LogEvents.Discount.EditStarted,
            "Editing discount. DiscountId: {DiscountId}",
            discountId);

        var discount =
            await _discountRepositoryContract
                .GetDiscountByIdAsync(discountId);

        if (discount is null)
        {
            _logger.LogWarning(
                LogEvents.Discount.NotFound,
                "Discount edit rejected because the discount was not found. DiscountId: {DiscountId}",
                discountId);

            throw new NotFoundException(
                "تخفیف یافت نشد .");
        }

        discount.Edit(
            title: dto.Title,
            discountType: dto.DiscountType,
            value: dto.Value,
            maxDiscountAmount: dto.MaxDiscountAmount,
            startsAt: dto.StartsAt,
            endsAt: dto.EndsAt
        );

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Discount.EditCompleted,
            "Discount edited successfully. DiscountId: {DiscountId}",
            discountId);

        return "تخفیف با موفقیت تغییر یافت .";
    }

    public async Task<string> DeleteDiscountByIdAsync(Guid discountId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(DeleteDiscountByIdAsync));

        activity?.SetTag("discount.id", discountId);

        if (discountId == Guid.Empty)
            throw new BusinessException(
                "شناسه تخفیف معتبر نیست .");

        _logger.LogInformation(
            LogEvents.Discount.DeleteStarted,
            "Deleting discount. DiscountId: {DiscountId}",
            discountId);

        var discount =
            await _discountRepositoryContract
                .GetDiscountByIdAsync(discountId);

        if (discount is null)
        {
            _logger.LogWarning(
                LogEvents.Discount.NotFound,
                "Discount deletion rejected because the discount was not found. DiscountId: {DiscountId}",
                discountId);

            throw new NotFoundException(
                "تخفیف یافت نشد .");
        }

        discount.Delete();

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Discount.DeleteCompleted,
            "Discount deleted successfully. DiscountId: {DiscountId}",
            discountId);

        return "تخفیف با موفقیت حذف شد .";
    }

    public async Task<string> RestoreDiscountByIdAsync(Guid discountId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(RestoreDiscountByIdAsync));

        activity?.SetTag("discount.id", discountId);

        if (discountId == Guid.Empty)
            throw new BusinessException(
                "شناسه تخفیف معتبر نیست .");

        _logger.LogInformation(
            LogEvents.Discount.RestoreStarted,
            "Restoring discount. DiscountId: {DiscountId}",
            discountId);

        var discount =
            await _discountRepositoryContract
                .GetDiscountForAdminByIdAsync(discountId);

        if (discount is null)
        {
            _logger.LogWarning(
                LogEvents.Discount.NotFound,
                "Discount restore rejected because the discount was not found. DiscountId: {DiscountId}",
                discountId);

            throw new NotFoundException(
                "تخفیف یافت نشد .");
        }

        discount.Restore();

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Discount.RestoreCompleted,
            "Discount restored successfully. DiscountId: {DiscountId}",
            discountId);

        return "تخفیف با موفقیت بازیابی شد .";
    }

    public async Task<string> SetDiscountForProductAsync(
        Guid discountId,
        AddProductToDiscountDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(SetDiscountForProductAsync));

        activity?.SetTag("discount.id", discountId);

        if (discountId == Guid.Empty)
            throw new BusinessException(
                "شناسه تخفیف نامعتبر است.");

        if (dto.ProductIds is null || !dto.ProductIds.Any())
            throw new BusinessException(
                "حداقل یک محصول باید انتخاب شود.");

        _logger.LogInformation(
            LogEvents.Discount.SetForProductStarted,
            "Adding discount to products. DiscountId: {DiscountId}, ProductCount: {ProductCount}",
            discountId,
            dto.ProductIds.Count);

        var discount =
            await _discountRepositoryContract
                .GetDiscountByIdAsync(discountId);

        if (discount is null)
        {
            _logger.LogWarning(
                LogEvents.Discount.NotFound,
                "Setting discount for products rejected because the discount was not found. DiscountId: {DiscountId}",
                discountId);

            throw new NotFoundException(
                "تخفیف یافت نشد.");
        }

        var products =
            await _productRepositoryContract
                .GetProductsByIdsAsync(dto.ProductIds);

        if (products is null || !products.Any())
            throw new NotFoundException(
                "محصولی یافت نشد.");

        var existingProductIds =
            await _discountProductRepositoryContract
                .GetExistingDiscountProductsAsync(
                    discountId,
                    dto.ProductIds);

        if (existingProductIds.Any())
            throw new BusinessException(
                "برخی محصولات قبلا این تخفیف را دارند.");

        await _unitOfWorkContract.BeginTransactionAsync();

        try
        {
            foreach (var product in products)
            {
                var discountProduct =
                    new Domain.Entities.DiscountProduct(
                        discountId,
                        product.Id);

                await _discountProductRepositoryContract
                    .AddProductToDiscountAsync(discountProduct);
            }

            await _unitOfWorkContract.SaveAsync();
            await _unitOfWorkContract.CommitTransactionAsync();

            activity?.SetTag(
                "discount.products_count",
                products.Count);

            _logger.LogInformation(
                LogEvents.Discount.SetForProductCompleted,
                "Discount added to products successfully. DiscountId: {DiscountId}, ProductCount: {ProductCount}",
                discountId,
                products.Count);
        }
        catch (Exception ex)
        {
            await _unitOfWorkContract.RollbackTransactionAsync();
            _unitOfWorkContract.ClearChangeTracker();

            _logger.LogError(
                LogEvents.Discount.Failed,
                ex,
                "Failed to add discount to products. DiscountId: {DiscountId}",
                discountId);

            throw;
        }

        return "تخفیف با موفقیت به محصولات اضافه شد.";
    }

    public async Task<string> SetDiscountForProductVariantAsync(
        Guid discountId,
        AddProductVariantToDiscountDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(SetDiscountForProductVariantAsync));

        activity?.SetTag("discount.id", discountId);

        if (discountId == Guid.Empty)
            throw new BusinessException(
                "شناسه تخفیف نامعتبر است.");

        if (dto.ProductVariantIds is null ||
            !dto.ProductVariantIds.Any())
            throw new BusinessException(
                "حداقل یک محصول باید انتخاب شود.");

        _logger.LogInformation(
            LogEvents.Discount.SetForVariantStarted,
            "Adding discount to product variants. DiscountId: {DiscountId}, VariantCount: {VariantCount}",
            discountId,
            dto.ProductVariantIds.Count);

        var discount =
            await _discountRepositoryContract
                .GetDiscountByIdAsync(discountId);

        if (discount is null)
        {
            _logger.LogWarning(
                LogEvents.Discount.NotFound,
                "Setting discount for variants rejected because the discount was not found. DiscountId: {DiscountId}",
                discountId);

            throw new NotFoundException(
                "تخفیف یافت نشد.");
        }

        var variants =
            await _productRepositoryContract
                .GetProductVariantsByIdsAsync(dto.ProductVariantIds);

        if (variants is null || !variants.Any())
            throw new NotFoundException(
                "محصولی یافت نشد.");

        var existingProductVariantIds =
            await _discountVariantRepositoryContract
                .GetExistingDiscountVariantsAsync(
                    discountId,
                    dto.ProductVariantIds);

        if (existingProductVariantIds.Any())
            throw new BusinessException(
                "برخی محصولات قبلا این تخفیف را دارند.");

        await _unitOfWorkContract.BeginTransactionAsync();

        try
        {
            foreach (var product in variants)
            {
                var discountVariant =
                    new Domain.Entities.DiscountVariant(
                        discountId,
                        product.Id);

                await _discountVariantRepositoryContract
                    .AddProductToDiscountAsync(discountVariant);
            }

            await _unitOfWorkContract.SaveAsync();
            await _unitOfWorkContract.CommitTransactionAsync();

            activity?.SetTag(
                "discount.variants_count",
                variants.Count);

            _logger.LogInformation(
                LogEvents.Discount.SetForVariantCompleted,
                "Discount added to product variants successfully. DiscountId: {DiscountId}, VariantCount: {VariantCount}",
                discountId,
                variants.Count);
        }
        catch (Exception ex)
        {
            await _unitOfWorkContract.RollbackTransactionAsync();
            _unitOfWorkContract.ClearChangeTracker();

            _logger.LogError(
                LogEvents.Discount.Failed,
                ex,
                "Failed to add discount to product variants. DiscountId: {DiscountId}",
                discountId);

            throw;
        }

        return "تخفیف با موفقیت به محصولات اضافه شد.";
    }

    public async Task<string> DeleteDiscountForProduct(
        Guid discountId,
        Guid productId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(DeleteDiscountForProduct));

        activity?.SetTag("discount.id", discountId);
        activity?.SetTag("product.id", productId);

        if (discountId == Guid.Empty)
            throw new BusinessException(
                "شناسه تخفیف نامعتبر است.");

        if (productId == Guid.Empty)
            throw new BusinessException(
                "شناسه محصول نامعتبر است.");

        _logger.LogInformation(
            LogEvents.Discount.RemoveFromProductStarted,
            "Removing discount from product. DiscountId: {DiscountId}, ProductId: {ProductId}",
            discountId,
            productId);

        var discount =
            await _discountRepositoryContract
                .GetDiscountByIdAsync(discountId);

        if (discount is null)
            throw new NotFoundException(
                "تخفیف یافت نشد.");

        var product =
            await _productRepositoryContract
                .GetProductByIdAsync(productId);

        if (product is null)
            throw new NotFoundException(
                "محصول یافت نشد .");

        var discountProduct =
            await _discountProductRepositoryContract
                .GetDiscountProductAsync(
                    discountId,
                    productId);

        if (discountProduct is null)
            throw new NotFoundException(
                "این تخفیف برای محصول موردنظر وجود ندارد.");

        await _discountProductRepositoryContract
            .RemoveAsync(discountProduct);

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Discount.RemoveFromProductCompleted,
            "Discount removed from product successfully. DiscountId: {DiscountId}, ProductId: {ProductId}",
            discountId,
            productId);

        return "تخفیف با موفقیت از محصول برداشته شد .";
    }

    public async Task<string> DeleteDiscountForProductVariant(
        Guid discountId,
        Guid productVariantId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(DeleteDiscountForProductVariant));

        activity?.SetTag("discount.id", discountId);
        activity?.SetTag("product_variant.id", productVariantId);

        if (discountId == Guid.Empty)
            throw new BusinessException(
                "شناسه تخفیف نامعتبر است.");

        if (productVariantId == Guid.Empty)
            throw new BusinessException(
                "شناسه محصول نامعتبر است.");

        _logger.LogInformation(
            LogEvents.Discount.RemoveFromVariantStarted,
            "Removing discount from product variant. DiscountId: {DiscountId}, ProductVariantId: {ProductVariantId}",
            discountId,
            productVariantId);

        var discount =
            await _discountRepositoryContract
                .GetDiscountByIdAsync(discountId);

        if (discount is null)
            throw new NotFoundException(
                "تخفیف یافت نشد.");

        var product =
            await _productRepositoryContract
                .GetProductByIdAsync(productVariantId);

        if (product is null)
            throw new NotFoundException(
                "محصول یافت نشد .");

        var discountVariant =
            await _discountVariantRepositoryContract
                .GetDiscountProductAsync(
                    discountId,
                    productVariantId);

        if (discountVariant is null)
            throw new NotFoundException(
                "این تخفیف برای محصول موردنظر وجود ندارد.");

        await _discountVariantRepositoryContract
            .RemoveAsync(discountVariant);

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Discount.RemoveFromVariantCompleted,
            "Discount removed from product variant successfully. DiscountId: {DiscountId}, ProductVariantId: {ProductVariantId}",
            discountId,
            productVariantId);

        return "تخفیف با موفقیت از محصول برداشته شد .";
    }

    public async Task<ViewDiscountItemsDto> GetDiscountByProductId(
        Guid productId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(GetDiscountByProductId));

        activity?.SetTag("product.id", productId);

        if (productId == Guid.Empty)
            throw new BusinessException(
                "شناسه محصول نامعتبر است .");

        var discount =
            await _discountProductRepositoryContract
                .GetDiscountByProductIdAsync(productId);

        if (discount is null)
        {
            _logger.LogWarning(
                LogEvents.Discount.NotFound,
                "No discount was found for product. ProductId: {ProductId}",
                productId);

            throw new NotFoundException(
                "تخفیفی برای محصول پیدا نشد .");
        }

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

    public async Task<ViewDiscountItemsDto> GetDiscountByProductVariantId(
        Guid productVariantId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(GetDiscountByProductVariantId));

        activity?.SetTag(
            "product_variant.id",
            productVariantId);

        if (productVariantId == Guid.Empty)
            throw new BusinessException(
                "شناسه محصول نامعتبر است .");

        var discount =
            await _discountVariantRepositoryContract
                .GetDiscountByProductVariantIdAsync(
                    productVariantId);

        if (discount is null)
        {
            _logger.LogWarning(
                LogEvents.Discount.NotFound,
                "No discount was found for product variant. ProductVariantId: {ProductVariantId}",
                productVariantId);

            throw new NotFoundException(
                "تخفیفی برای محصول پیدا نشد .");
        }

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