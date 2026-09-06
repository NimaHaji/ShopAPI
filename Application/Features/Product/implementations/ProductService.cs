using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Application.Caching.Interfaces;
using Application.Common.Interfaces;
using Application.Common.Observability;
using Application.Features.Inventory.Interfaces;
using Application.Features.Product.DTOs;
using Application.Features.Product.Interfaces;
using Application.Features.Review.DTOs;
using Application.Features.Review.interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Exceptions;

namespace Application.Features.Product.implementations;

public class ProductService : ProductServicesContract
{
    private readonly ProductRepositoryContract _productRepositoryContract;
    private readonly InventoryServiceContract _inventoryServiceContract;
    private readonly UnitOfWorkContract _unitOfWorkContract;
    private readonly SkuGeneratorContract _skuGeneratorContract;
    private readonly ReviewsRepositoryContract _reviewsRepositoryContract;
    private readonly IUSerContext _userContext;
    private readonly ILogger<ProductService> _logger;
    private readonly CacheKeyBuilderContract _cacheKeyBuilder;
    private readonly ICacheService _cacheService;

    public ProductService(ProductRepositoryContract productRepositoryContract,
        InventoryServiceContract inventoryServiceContract, UnitOfWorkContract unitOfWorkContract,
        SkuGeneratorContract skuGeneratorContract, ReviewsRepositoryContract reviewsRepositoryContract,
        IUSerContext userContext, ILogger<ProductService> logger, ICacheService cacheService,
        CacheKeyBuilderContract cacheKeyBuilder)
    {
        _productRepositoryContract = productRepositoryContract;
        _inventoryServiceContract = inventoryServiceContract;
        _unitOfWorkContract = unitOfWorkContract;
        _skuGeneratorContract = skuGeneratorContract;
        _reviewsRepositoryContract = reviewsRepositoryContract;
        _userContext = userContext;
        _logger = logger;
        _cacheService = cacheService;
        _cacheKeyBuilder = cacheKeyBuilder;
    }

    #region product

    public async Task<ViewProductDto> GetAllProducts(ProductQueryDto query)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(GetAllProducts));

        _logger.LogInformation(
            LogEvents.Product.GetAllStarted,
            "Getting products.");

        var cacheKey = _cacheKeyBuilder.ProductList(query);
        var productCache = await _cacheService.GetAsync<ViewProductDto>(cacheKey);

        if (productCache is not null)
        {
            activity?.SetTag("cache.hit", true);

            _logger.LogInformation("product successfully retrieved from cache. ProductCount: {ProductCount}",
                productCache.Items.Count);

            return productCache;
        }

        activity?.SetTag("cache.hit", false);

        var products = await _productRepositoryContract.GetProductList(query);

        if (products is null)
        {
            _logger.LogInformation(
                LogEvents.Product.GetAllCompleted,
                "No products found.");

            return new ViewProductDto
            {
                Items = []
            };
        }

        var now = DateTime.UtcNow;

        var result = new ViewProductDto
        {
            Items = products.Select(p =>
            {
                var variants = p.Variants
                    .Where(v => !v.IsDeleted)
                    .ToList();

                var activeProductDiscount = p.DiscountProducts
                    .Select(dp => dp.Discount)
                    .FirstOrDefault(d =>
                        !d.IsDeleted &&
                        d.IsActive &&
                        d.StartsAt <= now &&
                        d.EndsAt > now);

                var variantDtos = variants.Select(v =>
                {
                    var activeVariantDiscount = v.DiscountVariants
                        .Select(dp => dp.Discount)
                        .FirstOrDefault(d =>
                            !d.IsDeleted &&
                            d.IsActive &&
                            d.StartsAt <= now &&
                            d.EndsAt > now);

                    var discount = activeVariantDiscount ?? activeProductDiscount;

                    var priceInfo = CalculatePrice(v.Price, discount);

                    return new ViewProductVariantDto
                    {
                        Id = v.Id,
                        Sku = v.Sku,
                        Price = v.Price,
                        FinalPrice = priceInfo.FinalPrice,

                        Stock = v.InventoryItem?.AvailableQuantity ?? 0,

                        DiscountType = priceInfo.DiscountType,
                        DiscountPercentage = priceInfo.DiscountPercentage,
                        DiscountAmount = priceInfo.DiscountAmount,

                        Options = v.Options
                            .Select(pvo => new ViewProductVariantOptionDto
                            {
                                Id = pvo.Id,
                                ProductOptionId = pvo.ProductOptionId,
                                OptionName = pvo.ProductOption.Name,
                                ProductOptionValueId = pvo.ProductOptionValueId,
                                Value = pvo.ProductOptionValue.Value
                            })
                            .ToList(),

                        Images = v.Images
                            .OrderBy(i => i.SortOrder)
                            .Select(i => new ViewProductImageDto
                            {
                                Id = i.Id,
                                Url = i.ImageUrl,
                                IsPrimary = i.IsPrimary,
                                SortOrder = i.SortOrder
                            })
                            .ToList()
                    };
                }).ToList();

                return new ViewProductItemDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Brand = p.Brand?.Title ?? "بدون برند",
                    Category = p.Category.Title,

                    MinPrice = variantDtos
                        .Select(v => v.Price)
                        .DefaultIfEmpty(0)
                        .Min(),

                    MaxPrice = variantDtos
                        .Select(v => v.Price)
                        .DefaultIfEmpty(0)
                        .Max(),

                    FinalMinPrice = variantDtos
                        .Select(v => v.FinalPrice)
                        .DefaultIfEmpty(0)
                        .Min(),

                    FinalMaxPrice = variantDtos
                        .Select(v => v.FinalPrice)
                        .DefaultIfEmpty(0)
                        .Max(),

                    Stock = variants
                        .Sum(v => v.InventoryItem?.AvailableQuantity ?? 0),

                    Images = p.Images
                        .OrderBy(pi => pi.SortOrder)
                        .Select(pi => new ViewProductImageDto
                        {
                            Id = pi.Id,
                            IsPrimary = pi.IsPrimary,
                            SortOrder = pi.SortOrder,
                            Url = pi.ImageLink
                        })
                        .ToList(),

                    Rating = p.Reviews
                        .Where(r =>
                            !r.IsDeleted &&
                            r.ReviewStatus == ReviewStatus.Approved)
                        .Select(r => (decimal?)r.StarsCount)
                        .Average() ?? 0,

                    ReviewCount = p.Reviews
                        .Count(r =>
                            !r.IsDeleted &&
                            r.ReviewStatus == ReviewStatus.Approved),

                    Options = p.Options
                        .Select(o => new ViewProductOptionDto
                        {
                            Id = o.Id,
                            Name = o.Name,

                            Values = o.Values
                                .Select(pov => new ViewProductOptionValueDto
                                {
                                    Id = pov.Id,
                                    Value = pov.Value
                                })
                                .ToList()
                        })
                        .ToList(),

                    Variants = variantDtos
                };
            }).ToList()
        };

        await _cacheService.SetAsync(
            key: cacheKey,
            value: result,
            expiration: TimeSpan.FromMinutes(10)
        );

        activity?.SetTag("products.count", result.Items.Count);

        _logger.LogInformation(
            LogEvents.Product.GetAllCompleted,
            "Products retrieved successfully. ProductCount: {ProductCount}",
            result.Items.Count);

        return result;
    }

    public async Task<string> AddProductAsync(CreateProductDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(AddProductAsync));

        _logger.LogInformation(
            LogEvents.Product.CreateStarted,
            "Creating product.");

        var isExist =
            await _productRepositoryContract.IsExistingProduct(dto.Title);

        if (isExist)
        {
            _logger.LogWarning(
                LogEvents.Product.Duplicate,
                "Product creation rejected because the product already exists.");

            throw new DuplicateNameException(
                "این محصول از قبل وجود دارد");
        }

        var product = Domain.Entities.Product.Create(
            title: dto.Title,
            description: dto.Description,
            categoryId: dto.CategoryId,
            brandId: dto.BrandId ?? null
        );

        await _productRepositoryContract.CreateProductAsync(product);

        var sku = _skuGeneratorContract.GenerateSku();

        var variant = ProductVariant.Create(
            productId: product.Id,
            sku: sku,
            price: dto.Price
        );

        await _productRepositoryContract.AddProductVariantAsync(variant);

        await _inventoryServiceContract.AddStockAsync(
            product.Id,
            dto.Quantity,
            product.Description);

        await _unitOfWorkContract.SaveAsync();

        activity?.SetTag("product.id", product.Id);
        activity?.SetTag("product.variant_id", variant.Id);

        await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.ProductListPattern());
        await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.ProductSearchPattern());

        _logger.LogInformation(
            LogEvents.Product.CreateCompleted,
            "Product created successfully. ProductId: {ProductId}, VariantId: {VariantId}",
            product.Id,
            variant.Id);

        return $"محصول {product.Title} ساخته شد";
    }

    public async Task<SearchProductResultDto> SearchProductByTitle(string query)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(SearchProductByTitle));

        if (string.IsNullOrEmpty(query))
        {
            return new SearchProductResultDto
            {
                Items = []
            };
        }

        _logger.LogInformation(
            LogEvents.Product.SearchStarted,
            "Searching products by title.");

        var cacheKey = _cacheKeyBuilder.ProductSearch(query);
        var cacheProducts = await _cacheService.GetAsync<SearchProductResultDto>(cacheKey);

        if (cacheProducts is not null)
        {
            activity?.SetTag("cache.hit", true);

            _logger.LogInformation(
                LogEvents.Product.SearchCompleted,
                "Product search retrieved successfully from cache.ResultCount: {ResultCount}",
                cacheProducts.Items.Count
            );

            return cacheProducts;
        }

        activity?.SetTag("cache.hit", false);

        var productList =
            await _productRepositoryContract.SearchProductWithTitle(query);

        if (productList is null)
        {
            _logger.LogInformation(
                LogEvents.Product.SearchCompleted,
                "Product search completed with no results.");

            return new SearchProductResultDto
            {
                Items = []
            };
        }

        var result = new SearchProductResultDto
        {
            Items = productList
                .Select(x => new SearchProductItemsResultDto
                {
                    Title = x.Title,
                    Category = x.Category.Title,
                })
                .ToList()
        };

        await _cacheService.SetAsync(
            key: cacheKey,
            value: result,
            expiration: TimeSpan.FromMinutes(10)
        );

        activity?.SetTag("products.count", result.Items.Count);

        _logger.LogInformation(
            LogEvents.Product.SearchCompleted,
            "Product search completed successfully. ResultCount: {ResultCount}",
            result.Items.Count);

        return result;
    }

    public async Task<ViewProductItemDto> GetProductById(Guid productId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(GetProductById));

        activity?.SetTag("product.id", productId);

        var cacheKey = _cacheKeyBuilder.Product(productId);
        var cachedProduct = await _cacheService.GetAsync<ViewProductItemDto>(cacheKey);

        if (cachedProduct is not null)
        {
            activity?.SetTag("cache.hit", true);

            _logger.LogInformation(
                LogEvents.Product.GetByIdCompleted,
                "Product retrieved successfully from cache.");

            return cachedProduct;
        }

        activity?.SetTag("cache.hit", false);

        var product =
            await _productRepositoryContract.GetProductByIdAsync(productId);

        if (product is null)
        {
            _logger.LogWarning(
                LogEvents.Product.NotFound,
                "Product not found. ProductId: {ProductId}",
                productId);

            throw new NotFoundException("محصول یافت نشد.");
        }

        var (rating, reviewCount) =
            await _reviewsRepositoryContract.GetProductRatingAsync(productId);

        var now = DateTime.UtcNow;

        var variants = product.Variants
            .Where(v => !v.IsDeleted)
            .ToList();

        var activeProductDiscount = product.DiscountProducts
            .Select(dp => dp.Discount)
            .FirstOrDefault(d =>
                !d.IsDeleted &&
                d.IsActive &&
                d.StartsAt <= now &&
                d.EndsAt > now);

        var variantDto = variants.Select(v =>
        {
            var activeVariantDiscount = v.DiscountVariants
                .Select(dp => dp.Discount)
                .FirstOrDefault(d =>
                    !d.IsDeleted &&
                    d.IsActive &&
                    d.StartsAt <= now &&
                    d.EndsAt > now);

            var discount = activeVariantDiscount ?? activeProductDiscount;

            var priceInfo = CalculatePrice(v.Price, discount);

            return new ViewProductVariantDto
            {
                Id = v.Id,
                Sku = v.Sku,
                Price = v.Price,
                FinalPrice = priceInfo.FinalPrice,

                Stock = v.InventoryItem?.AvailableQuantity ?? 0,

                DiscountType = priceInfo.DiscountType,
                DiscountPercentage = priceInfo.DiscountPercentage,
                DiscountAmount = priceInfo.DiscountAmount,

                Options = v.Options
                    .Select(pvo => new ViewProductVariantOptionDto
                    {
                        Id = pvo.Id,
                        ProductOptionId = pvo.ProductOptionId,
                        OptionName = pvo.ProductOption.Name,
                        ProductOptionValueId = pvo.ProductOptionValueId,
                        Value = pvo.ProductOptionValue.Value
                    })
                    .ToList(),

                Images = v.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new ViewProductImageDto
                    {
                        Id = i.Id,
                        Url = i.ImageUrl,
                        IsPrimary = i.IsPrimary,
                        SortOrder = i.SortOrder
                    })
                    .ToList()
            };
        }).ToList();

        var prices = variantDto
            .Select(v => v.Price)
            .ToList();

        var finalPrices = variantDto
            .Select(v => v.FinalPrice)
            .ToList();

        activity?.SetTag("product.variant_count", variantDto.Count);
        activity?.SetTag("product.rating", rating);
        activity?.SetTag("product.review_count", reviewCount);

        _logger.LogInformation(
            LogEvents.Product.GetByIdCompleted,
            "Product retrieved successfully. ProductId: {ProductId}",
            productId);

        var result = new ViewProductItemDto
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Brand = product.Brand?.Title ?? "بدون برند",
            Category = product.Category.Title,

            MinPrice = prices.DefaultIfEmpty(0).Min(),
            MaxPrice = prices.DefaultIfEmpty(0).Max(),

            FinalMinPrice = finalPrices.DefaultIfEmpty(0).Min(),
            FinalMaxPrice = finalPrices.DefaultIfEmpty(0).Max(),

            Stock = variants
                .Sum(v => v.InventoryItem?.AvailableQuantity ?? 0),

            Images = product.Images
                .OrderBy(pi => pi.SortOrder)
                .Select(pi => new ViewProductImageDto
                {
                    Id = pi.Id,
                    IsPrimary = pi.IsPrimary,
                    SortOrder = pi.SortOrder,
                    Url = pi.ImageLink
                })
                .ToList(),

            Rating = product.Reviews
                .Where(r =>
                    !r.IsDeleted &&
                    r.ReviewStatus == ReviewStatus.Approved)
                .Select(r => (decimal?)r.StarsCount)
                .Average() ?? 0,

            ReviewCount = product.Reviews
                .Count(r =>
                    !r.IsDeleted &&
                    r.ReviewStatus == ReviewStatus.Approved),

            Options = product.Options
                .Select(o => new ViewProductOptionDto
                {
                    Id = o.Id,
                    Name = o.Name,

                    Values = o.Values
                        .Select(pov => new ViewProductOptionValueDto
                        {
                            Id = pov.Id,
                            Value = pov.Value
                        })
                        .ToList()
                })
                .ToList(),

            Variants = variantDto
        };

        await _cacheService.SetAsync(
            key: cacheKey,
            value: result,
            expiration: TimeSpan.FromMinutes(10));

        return result;
    }

    public async Task<string> EditProductAsync(EditProductDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(EditProductAsync));

        activity?.SetTag("product.id", dto.Id);

        if (dto.Title is null &&
            dto.Description is null)
        {
            _logger.LogWarning(
                LogEvents.Product.EditRejected,
                "Product edit rejected because no field was provided. ProductId: {ProductId}",
                dto.Id);

            throw new BusinessException(
                "برای ویرایش محصول، حداقل یک فیلد را تغییر دهید.");
        }

        int attempt = 0;
        const int maxAttempts = 5;

        _logger.LogInformation(
            LogEvents.Product.EditStarted,
            "Editing product. ProductId: {ProductId}",
            dto.Id);

        while (maxAttempts > attempt)
        {
            var product =
                await _productRepositoryContract.GetProductByIdAsync(dto.Id);

            if (product is null)
            {
                _logger.LogWarning(
                    LogEvents.Product.NotFound,
                    "Product not found for edit. ProductId: {ProductId}",
                    dto.Id);

                throw new NotFoundException("محصولی یافت نشد!");
            }

            try
            {
                product.Edit(dto.Title, dto.Description);

                await _unitOfWorkContract.SaveAsync();

                await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.ProductListPattern());
                await _cacheService.RemoveAsync(_cacheKeyBuilder.Product(product.Id));
                await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.ProductSearchPattern());

                _logger.LogInformation(
                    LogEvents.Product.EditCompleted,
                    "Product edited successfully. ProductId: {ProductId}",
                    dto.Id);

                return "محصول با موفقیت تغییر کرد";
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;

                _unitOfWorkContract.ClearChangeTracker();

                _logger.LogWarning(
                    LogEvents.Product.ConcurrencyConflict,
                    "Product edit concurrency conflict. ProductId: {ProductId}, Attempt: {Attempt}, MaxAttempts: {MaxAttempts}",
                    dto.Id,
                    attempt,
                    maxAttempts);

                if (attempt == maxAttempts)
                {
                    _logger.LogError(
                        LogEvents.Product.Failed,
                        "Product edit failed after maximum concurrency retry attempts. ProductId: {ProductId}, Attempts: {Attempts}",
                        dto.Id,
                        attempt);

                    throw new ConflictException(
                        "محصول در حال تغییر است. لطفا دوباره تلاش کنید");
                }
            }
        }

        throw new InvalidOperationException("خطای ناشناخته");
    }

    public async Task<string> DeleteProductAsync(Guid productId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(DeleteProductAsync));

        activity?.SetTag("product.id", productId);

        int attempt = 0;
        const int maxAttempts = 5;

        _logger.LogInformation(
            LogEvents.Product.DeleteStarted,
            "Deleting product. ProductId: {ProductId}",
            productId);

        while (maxAttempts > attempt)
        {
            var product =
                await _productRepositoryContract.GetProductByIdAsync(productId);

            if (product is null)
            {
                _logger.LogWarning(
                    LogEvents.Product.NotFound,
                    "Product not found for deletion. ProductId: {ProductId}",
                    productId);

                throw new NotFoundException("محصولی یافت نشد!");
            }

            try
            {
                product.Delete();

                await _unitOfWorkContract.SaveAsync();

                await _cacheService.RemoveAsync(_cacheKeyBuilder.Product(product.Id));
                await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.ProductSearchPattern());
                await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.ProductListPattern());

                _logger.LogInformation(
                    LogEvents.Product.DeleteCompleted,
                    "Product deleted successfully. ProductId: {ProductId}",
                    productId);

                return "محصول با موفقیت حذف شد";
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;

                _unitOfWorkContract.ClearChangeTracker();

                _logger.LogWarning(
                    LogEvents.Product.ConcurrencyConflict,
                    "Product deletion concurrency conflict. ProductId: {ProductId}, Attempt: {Attempt}, MaxAttempts: {MaxAttempts}",
                    productId,
                    attempt,
                    maxAttempts);

                if (attempt == maxAttempts)
                {
                    _logger.LogError(
                        LogEvents.Product.Failed,
                        "Product deletion failed after maximum concurrency retry attempts. ProductId: {ProductId}, Attempts: {Attempts}",
                        productId,
                        attempt);

                    throw new ConflictException(
                        "محصول در حال تغییر است. لطفا دوباره تلاش کنید");
                }
            }
        }

        throw new InvalidOperationException("خطای ناشناخته");
    }

    public async Task<string> RestoreProductAsync(Guid productId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(RestoreProductAsync));

        activity?.SetTag("product.id", productId);

        int attempt = 0;
        const int maxAttempts = 5;

        _logger.LogInformation(
            LogEvents.Product.RestoreStarted,
            "Restoring product. ProductId: {ProductId}",
            productId);

        while (maxAttempts > attempt)
        {
            var product =
                await _productRepositoryContract.GetProductByIdAsync(productId);

            if (product is null)
            {
                _logger.LogWarning(
                    LogEvents.Product.NotFound,
                    "Product not found for restore. ProductId: {ProductId}",
                    productId);

                throw new NotFoundException("محصولی یافت نشد!");
            }

            try
            {
                product.Restore();

                await _unitOfWorkContract.SaveAsync();

                await _cacheService.RemoveAsync(_cacheKeyBuilder.Product(product.Id));
                await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.ProductSearchPattern());
                await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.ProductListPattern());

                _logger.LogInformation(
                    LogEvents.Product.RestoreCompleted,
                    "Product restored successfully. ProductId: {ProductId}",
                    productId);

                return "محصول با موفقیت بازیابی شد";
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;

                _unitOfWorkContract.ClearChangeTracker();

                _logger.LogWarning(
                    LogEvents.Product.ConcurrencyConflict,
                    "Product restore concurrency conflict. ProductId: {ProductId}, Attempt: {Attempt}, MaxAttempts: {MaxAttempts}",
                    productId,
                    attempt,
                    maxAttempts);

                if (attempt == maxAttempts)
                {
                    _logger.LogError(
                        LogEvents.Product.Failed,
                        "Product restore failed after maximum concurrency retry attempts. ProductId: {ProductId}, Attempts: {Attempts}",
                        productId,
                        attempt);

                    throw new ConflictException(
                        "محصول در حال تغییر است. لطفا دوباره تلاش کنید");
                }
            }
        }

        throw new InvalidOperationException("خطای ناشناخته");
    }

    private static PriceCalculationResult CalculatePrice(long price, Domain.Entities.Discount? discount)
    {
        if (discount is null)
        {
            return new PriceCalculationResult(
                FinalPrice: price,
                DiscountAmount: null,
                DiscountPercentage: null,
                DiscountType: null);
        }

        long discountAmount = 0;
        decimal? discountPercentage = null;

        if (discount.DiscountType == DiscountType.Percentage)
        {
            discountPercentage = discount.Value;

            var calculatedDiscount =
                price * (discount.Value / 100);

            if (discount.MaxDiscountAmount.HasValue)
            {
                calculatedDiscount = Math.Min(
                    calculatedDiscount,
                    discount.MaxDiscountAmount.Value);
            }

            discountAmount = (long)calculatedDiscount;
        }
        else if (discount.DiscountType == DiscountType.FixedAmount)
        {
            discountAmount = (long)discount.Value;
        }

        var finalPrice = Math.Max(
            0,
            price - discountAmount);

        return new PriceCalculationResult(
            FinalPrice: finalPrice,
            DiscountAmount: discountAmount,
            DiscountPercentage: discountPercentage,
            DiscountType: discount.DiscountType);
    }

    #endregion

    #region category

    public async Task<string> DeleteProductCategoryAsync(Guid productCategoryId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(DeleteProductCategoryAsync));

        activity?.SetTag("product_category.id", productCategoryId);

        int attempt = 0;
        const int maxAttempts = 5;

        _logger.LogInformation(
            LogEvents.ProductCategory.DeleteStarted,
            "Product category deletion started. ProductCategoryId: {ProductCategoryId}",
            productCategoryId);

        while (maxAttempts > attempt)
        {
            var productCategory =
                await _productRepositoryContract
                    .GetProductCategoryById(productCategoryId);

            if (productCategory is null)
            {
                _logger.LogWarning(
                    LogEvents.ProductCategory.NotFound,
                    "Product category was not found. ProductCategoryId: {ProductCategoryId}",
                    productCategoryId);

                throw new NotFoundException(
                    "دسته بندی محصولی یافت نشد !");
            }

            try
            {
                productCategory.Delete();

                await _unitOfWorkContract.SaveAsync();

                await _cacheService.RemoveAsync(_cacheKeyBuilder.CategoryList());
                await _cacheService.RemoveAsync(_cacheKeyBuilder.Category(productCategoryId));
                await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.CategorySearchPattern());

                _logger.LogInformation(
                    LogEvents.ProductCategory.DeleteCompleted,
                    "Product category deleted successfully. ProductCategoryId: {ProductCategoryId}",
                    productCategoryId);

                return "دسته بندی محصول با موفقیت حذف شد";
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;

                _unitOfWorkContract.ClearChangeTracker();

                _logger.LogWarning(
                    LogEvents.ProductCategory.ConcurrencyConflict,
                    "Product category deletion concurrency conflict. ProductCategoryId: {ProductCategoryId}, Attempt: {Attempt}, MaxAttempts: {MaxAttempts}",
                    productCategoryId,
                    attempt,
                    maxAttempts);

                if (attempt == maxAttempts)
                {
                    _logger.LogError(
                        LogEvents.ProductCategory.Failed,
                        "Product category deletion failed after maximum concurrency retry attempts. ProductCategoryId: {ProductCategoryId}, Attempts: {Attempts}",
                        productCategoryId,
                        attempt);

                    throw new ConflictException(
                        "دسته بندی محصول در حال تغییر است . لطفا دوباره تلاش کنید");
                }
            }
        }

        throw new InvalidOperationException("خطای ناشناخته");
    }

    public async Task<string> RestoreProductCategoryAsync(Guid productCategoryId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(RestoreProductCategoryAsync));

        activity?.SetTag("product_category.id", productCategoryId);

        int attempt = 0;
        const int maxAttempts = 5;

        _logger.LogInformation(
            LogEvents.ProductCategory.RestoreStarted,
            "Product category restore started. ProductCategoryId: {ProductCategoryId}",
            productCategoryId);

        while (maxAttempts > attempt)
        {
            var productCategory =
                await _productRepositoryContract
                    .GetProductCategoryById(productCategoryId);

            if (productCategory is null)
            {
                _logger.LogWarning(
                    LogEvents.ProductCategory.NotFound,
                    "Product category was not found. ProductCategoryId: {ProductCategoryId}",
                    productCategoryId);

                throw new NotFoundException(
                    "دسته بندی محصولی یافت نشد !");
            }

            try
            {
                productCategory.Restore();

                await _unitOfWorkContract.SaveAsync();

                await _cacheService.RemoveAsync(_cacheKeyBuilder.CategoryList());
                await _cacheService.RemoveAsync(_cacheKeyBuilder.Category(productCategoryId));
                await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.CategorySearchPattern());

                _logger.LogInformation(
                    LogEvents.ProductCategory.RestoreCompleted,
                    "Product category restored successfully. ProductCategoryId: {ProductCategoryId}",
                    productCategoryId);

                return "دسته بندی محصول با موفقیت بازیابی شد";
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;

                _unitOfWorkContract.ClearChangeTracker();

                _logger.LogWarning(
                    LogEvents.ProductCategory.ConcurrencyConflict,
                    "Product category restore concurrency conflict. ProductCategoryId: {ProductCategoryId}, Attempt: {Attempt}, MaxAttempts: {MaxAttempts}",
                    productCategoryId,
                    attempt,
                    maxAttempts);

                if (attempt == maxAttempts)
                {
                    _logger.LogError(
                        LogEvents.ProductCategory.Failed,
                        "Product category restore failed after maximum concurrency retry attempts. ProductCategoryId: {ProductCategoryId}, Attempts: {Attempts}",
                        productCategoryId,
                        attempt);

                    throw new ConflictException(
                        "دسته بندی محصول در حال تغییر است . لطفا دوباره تلاش کنید");
                }
            }
        }

        throw new InvalidOperationException("خطای ناشناخته");
    }

    public async Task<ViewProductCategoryDto> GetAllCategories()
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(GetAllCategories));

        _logger.LogInformation(
            LogEvents.ProductCategory.GetAllStarted,
            "Product categories retrieval started.");

        var cacheKey = _cacheKeyBuilder.CategoryList();

        var cachedCategories = await _cacheService.GetAsync<ViewProductCategoryDto>(cacheKey);

        if (cachedCategories is not null)
        {
            activity?.SetTag("cache.hit", true);

            _logger.LogInformation("Product categories retrieved from cache.Count: {Count}",
                cachedCategories.Items.Count);

            return cachedCategories;
        }

        activity?.SetTag("cache.hit", false);

        var categories =
            await _productRepositoryContract.GetAllProductCategories();

        var result = new ViewProductCategoryDto
        {
            Items = categories.Select(x => new ViewProductCategoryItemDto
            {
                Title = x.Title
            }).ToList()
        };

        await _cacheService.SetAsync(
            key: cacheKey,
            value: result,
            expiration: TimeSpan.FromMinutes(10)
        );

        activity?.SetTag("product_categories.count", result.Items.Count);

        _logger.LogInformation(
            LogEvents.ProductCategory.GetAllCompleted,
            "Product categories retrieved successfully. Count: {Count}",
            result.Items.Count);

        return result;
    }

    public async Task<ViewProductCategoryDto> SearchProductCategoryByTitle(
        SearchProductCategoryDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(SearchProductCategoryByTitle));

        activity?.SetTag("product_category.search_title", dto.Title);

        _logger.LogInformation(
            LogEvents.ProductCategory.SearchStarted,
            "Product category search started.");

        var search = dto.Title?.Trim();

        if (string.IsNullOrWhiteSpace(search))
        {
            return new ViewProductCategoryDto
            {
                Items = []
            };
        }

        var cacheKey = _cacheKeyBuilder.CategorySearch(search);

        var cachedCategories = await _cacheService.GetAsync<ViewProductCategoryDto>(cacheKey);

        if (cachedCategories is not null)
        {
            activity?.SetTag("cache.hit", true);

            _logger.LogInformation("Product category searched from cache.Count: {Count}",
                cachedCategories.Items.Count);

            return cachedCategories;
        }

        activity?.SetTag("cache.hit", false);

        var categories =
            await _productRepositoryContract
                .SearchProductCategoriesWithTitle(search);

        if (categories is null)
        {
            _logger.LogInformation(
                LogEvents.ProductCategory.SearchCompleted,
                "Product category search completed with no results.");

            var emptyResult = new ViewProductCategoryDto
            {
                Items = []
            };

            await _cacheService.SetAsync(
                key: cacheKey,
                value: emptyResult,
                expiration: TimeSpan.FromMinutes(5)
            );

            return emptyResult;
        }

        var result = new ViewProductCategoryDto
        {
            Items = categories
                .Select(x => new ViewProductCategoryItemDto
                {
                    Title = x.Title
                })
                .ToList()
        };

        await _cacheService.SetAsync(
            key: cacheKey,
            value: result,
            expiration: TimeSpan.FromMinutes(10)
        );

        activity?.SetTag("product_categories.count", result.Items.Count);

        _logger.LogInformation(
            LogEvents.ProductCategory.SearchCompleted,
            "Product category search completed. Count: {Count}",
            result.Items.Count);

        return result;
    }

    public async Task<ViewProductCategoryItemDto> GetProductCategoryById(
        Guid productCategoryId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(GetProductCategoryById));

        activity?.SetTag("product_category.id", productCategoryId);

        var cacheKey = _cacheKeyBuilder.Category(productCategoryId);
        var cachedCategory = await _cacheService.GetAsync<ViewProductCategoryItemDto>(cacheKey);

        if (cachedCategory is not null)
        {
            activity?.SetTag("cache.hit", true);
            _logger.LogInformation(
                "Product category retrieved from cache successfully.ProductCategoryId: {ProductCategoryId}",
                productCategoryId);

            return cachedCategory;
        }

        activity?.SetTag("cache.hit", false);

        var category =
            await _productRepositoryContract
                .GetProductCategoryById(productCategoryId);

        if (category is null)
        {
            _logger.LogWarning(
                LogEvents.ProductCategory.NotFound,
                "Product category was not found. ProductCategoryId: {ProductCategoryId}",
                productCategoryId);

            throw new NotFoundException(
                "دسته بندی محصول یافت نشد");
        }

        var dto = new ViewProductCategoryItemDto
        {
            Title = category.Title
        };

        await _cacheService.SetAsync(
            key: cacheKey,
            value: dto,
            expiration: TimeSpan.FromMinutes(10)
        );

        _logger.LogInformation(
            LogEvents.ProductCategory.GetByIdCompleted,
            "Product category retrieved successfully. ProductCategoryId: {ProductCategoryId}",
            productCategoryId);

        return dto;
    }

    public async Task<string> EditProductCategoryAsync(
        EditProductCategoryDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(EditProductCategoryAsync));

        activity?.SetTag("product_category.id", dto.Id);

        _logger.LogInformation(
            LogEvents.ProductCategory.EditStarted,
            "Product category edit started. ProductCategoryId: {ProductCategoryId}",
            dto.Id);

        var category =
            await _productRepositoryContract
                .GetProductCategoryById(dto.Id);

        if (category is null)
        {
            _logger.LogWarning(
                LogEvents.ProductCategory.NotFound,
                "Product category was not found for edit. ProductCategoryId: {ProductCategoryId}",
                dto.Id);

            throw new NotFoundException(
                "دسته بندی محصول یافت نشد .");
        }

        category.Edit(dto.Title);

        await _unitOfWorkContract.SaveAsync();

        await _cacheService.RemoveAsync(_cacheKeyBuilder.CategoryList());
        await _cacheService.RemoveAsync(_cacheKeyBuilder.Category(dto.Id));
        await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.CategorySearchPattern());

        _logger.LogInformation(
            LogEvents.ProductCategory.EditCompleted,
            "Product category edited successfully. ProductCategoryId: {ProductCategoryId}",
            dto.Id);

        return "دسته بندی محصول با موفقیت تغییر کرد .";
    }

    public async Task<string> CreateProductCategoryAsync(
        CreateProductCategoryDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(CreateProductCategoryAsync));

        _logger.LogInformation(
            LogEvents.ProductCategory.CreateStarted,
            "Product category creation started.");

        var isExist =
            await _productRepositoryContract
                .IsExistingProductCategory(dto.Title);

        if (isExist)
        {
            _logger.LogWarning(
                LogEvents.ProductCategory.Duplicate,
                "Product category creation rejected because the title already exists.");

            throw new DuplicateNameException(
                "دسته بندی محصول وجود دارد");
        }

        var category = ProductCategory.Create(dto.Title);

        await _productRepositoryContract.AddProductCategory(category);
        await _unitOfWorkContract.SaveAsync();

        await _cacheService.RemoveAsync(_cacheKeyBuilder.CategoryList());
        await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.CategorySearchPattern());

        activity?.SetTag(
            "product_category.id",
            category.Id);

        _logger.LogInformation(
            LogEvents.ProductCategory.CreateCompleted,
            "Product category created successfully. ProductCategoryId: {ProductCategoryId}",
            category.Id);

        return $"دسته بندی {category.Title} ساخته شد .";
    }

    #endregion

    #region Brand

    public async Task<ViewProductBrandDto> GetAllProductBrands()
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(GetAllProductBrands));

        _logger.LogInformation(
            LogEvents.ProductBrand.GetAllStarted,
            "Product brands retrieval started.");

        var cacheKey = _cacheKeyBuilder.BrandList();
        var cachedBrands = await _cacheService.GetAsync<ViewProductBrandDto>(cacheKey);

        if (cachedBrands is not null)
        {
            activity?.SetTag("cache.hit", true);

            _logger.LogInformation(
                LogEvents.ProductBrand.GetAllCompleted,
                "Product brands retrieved successfully from cache. Count: {Count}",
                cachedBrands.Items.Count
            );

            return cachedBrands;
        }

        activity?.SetTag("cache.hit", false);

        var brands =
            await _productRepositoryContract.GetAllBrandAsync();

        if (brands is null)
        {
            _logger.LogInformation(
                LogEvents.ProductBrand.GetAllCompleted,
                "Product brands retrieval completed with no results.");

            var emptyResult = new ViewProductBrandDto
            {
                Items = []
            };

            await _cacheService.SetAsync(
                key: cacheKey,
                value: emptyResult,
                expiration: TimeSpan.FromMinutes(10)
            );

            return emptyResult;
        }

        var result = new ViewProductBrandDto
        {
            Items = brands
                .Select(x => new ViewProductBrandItemDto
                {
                    Title = x.Title
                })
                .ToList()
        };

        await _cacheService.SetAsync(
            key: cacheKey,
            value: result,
            expiration: TimeSpan.FromMinutes(10)
        );

        activity?.SetTag("product_brands.count", result.Items.Count);

        _logger.LogInformation(
            LogEvents.ProductBrand.GetAllCompleted,
            "Product brands retrieved successfully. Count: {Count}",
            result.Items.Count);

        return result;
    }

    public async Task<string> CreateProductBrandAsync(
        CreateProductBrandDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(CreateProductBrandAsync));

        _logger.LogInformation(
            LogEvents.ProductBrand.CreateStarted,
            "Product brand creation started.");

        var isExist =
            await _productRepositoryContract.IsExistingBrand(dto.Title);

        if (isExist)
        {
            _logger.LogWarning(
                LogEvents.ProductBrand.Duplicate,
                "Product brand creation rejected because the title already exists.");

            throw new DuplicateNameException(
                "برند محصول وجود دارد");
        }

        var brand = ProductBrand.Create(dto.Title);

        await _productRepositoryContract.AddBrandAsync(brand);
        await _unitOfWorkContract.SaveAsync();

        await _cacheService.RemoveAsync(_cacheKeyBuilder.BrandList());
        await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.BrandSearchPattern());

        activity?.SetTag("product_brand.id", brand.Id);

        _logger.LogInformation(
            LogEvents.ProductBrand.CreateCompleted,
            "Product brand created successfully. ProductBrandId: {ProductBrandId}",
            brand.Id);

        return $"برند {brand.Title} ساخته شد .";
    }

    public async Task<string> DeleteProductBrandAsync(
        Guid productBrandId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(DeleteProductBrandAsync));

        activity?.SetTag("product_brand.id", productBrandId);

        int attempt = 0;
        const int maxAttempts = 5;

        _logger.LogInformation(
            LogEvents.ProductBrand.DeleteStarted,
            "Product brand deletion started. ProductBrandId: {ProductBrandId}",
            productBrandId);

        while (maxAttempts > attempt)
        {
            var productBrand =
                await _productRepositoryContract
                    .GetProductBrandById(productBrandId);

            if (productBrand is null)
            {
                _logger.LogWarning(
                    LogEvents.ProductBrand.NotFound,
                    "Product brand was not found. ProductBrandId: {ProductBrandId}",
                    productBrandId);

                throw new NotFoundException(
                    "برند محصول یافت نشد !");
            }

            try
            {
                var productIds = await _productRepositoryContract.GetProductIdsByBrandIdAsync(productBrandId);

                productBrand.Delete();

                await _unitOfWorkContract.SaveAsync();

                await _cacheService.RemoveAsync(_cacheKeyBuilder.Brand(productBrandId));
                await _cacheService.RemoveAsync(_cacheKeyBuilder.BrandList());
                await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.BrandSearchPattern());

                foreach (var productId in productIds)
                {
                    await _cacheService.RemoveAsync(_cacheKeyBuilder.Product(productId));
                }

                await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.ProductListPattern());

                _logger.LogInformation(
                    LogEvents.ProductBrand.DeleteCompleted,
                    "Product brand deleted successfully. ProductBrandId: {ProductBrandId}",
                    productBrandId);

                return "برند محصول با موفقیت حذف شد";
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;

                _unitOfWorkContract.ClearChangeTracker();

                _logger.LogWarning(
                    LogEvents.ProductBrand.ConcurrencyConflict,
                    "Product brand deletion concurrency conflict. ProductBrandId: {ProductBrandId}, Attempt: {Attempt}, MaxAttempts: {MaxAttempts}",
                    productBrandId,
                    attempt,
                    maxAttempts);

                if (attempt == maxAttempts)
                {
                    _logger.LogError(
                        LogEvents.ProductBrand.Failed,
                        "Product brand deletion failed after maximum concurrency retry attempts. ProductBrandId: {ProductBrandId}, Attempts: {Attempts}",
                        productBrandId,
                        attempt);

                    throw new ConflictException(
                        "برند محصول در حال تغییر است . لطفا دوباره تلاش کنید");
                }
            }
        }

        throw new InvalidOperationException("خطای ناشناخته");
    }

    public async Task<string> RestoreProductBrandAsync(
        Guid productBrandId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(RestoreProductBrandAsync));

        activity?.SetTag("product_brand.id", productBrandId);

        int attempt = 0;
        const int maxAttempts = 5;

        _logger.LogInformation(
            LogEvents.ProductBrand.RestoreStarted,
            "Product brand restore started. ProductBrandId: {ProductBrandId}",
            productBrandId);

        while (maxAttempts > attempt)
        {
            var productBrand =
                await _productRepositoryContract
                    .GetProductBrandById(productBrandId);

            if (productBrand is null)
            {
                _logger.LogWarning(
                    LogEvents.ProductBrand.NotFound,
                    "Product brand was not found. ProductBrandId: {ProductBrandId}",
                    productBrandId);

                throw new NotFoundException(
                    "برند محصول یافت نشد !");
            }

            try
            {
                var productIds = await _productRepositoryContract.GetProductIdsByBrandIdAsync(productBrandId);

                productBrand.Restore();

                await _unitOfWorkContract.SaveAsync();

                await _cacheService.RemoveAsync(_cacheKeyBuilder.Brand(productBrandId));
                await _cacheService.RemoveAsync(_cacheKeyBuilder.BrandList());
                await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.BrandSearchPattern());

                foreach (var productId in productIds)
                {
                    await _cacheService.RemoveAsync(_cacheKeyBuilder.Product(productId));
                }

                await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.ProductListPattern());

                _logger.LogInformation(
                    LogEvents.ProductBrand.RestoreCompleted,
                    "Product brand restored successfully. ProductBrandId: {ProductBrandId}",
                    productBrandId);

                return "برند محصول با موفقیت بازیابی شد";
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;

                _unitOfWorkContract.ClearChangeTracker();

                _logger.LogWarning(
                    LogEvents.ProductBrand.ConcurrencyConflict,
                    "Product brand restore concurrency conflict. ProductBrandId: {ProductBrandId}, Attempt: {Attempt}, MaxAttempts: {MaxAttempts}",
                    productBrandId,
                    attempt,
                    maxAttempts);

                if (attempt == maxAttempts)
                {
                    _logger.LogError(
                        LogEvents.ProductBrand.Failed,
                        "Product brand restore failed after maximum concurrency retry attempts. ProductBrandId: {ProductBrandId}, Attempts: {Attempts}",
                        productBrandId,
                        attempt);

                    throw new ConflictException(
                        "برند محصول در حال تغییر است . لطفا دوباره تلاش کنید");
                }
            }
        }

        throw new InvalidOperationException("خطای ناشناخته");
    }

    public async Task<string> EditProductBrandAsync(
        EditProductBrandDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(EditProductBrandAsync));

        activity?.SetTag("product_brand.id", dto.Id);

        _logger.LogInformation(
            LogEvents.ProductBrand.EditStarted,
            "Product brand edit started. ProductBrandId: {ProductBrandId}",
            dto.Id);

        var brand =
            await _productRepositoryContract
                .GetProductBrandById(dto.Id);

        if (brand is null)
        {
            _logger.LogWarning(
                LogEvents.ProductBrand.NotFound,
                "Product brand was not found for edit. ProductBrandId: {ProductBrandId}",
                dto.Id);

            throw new NotFoundException(
                "برند محصول یافت نشد .");
        }

        var productIds = await _productRepositoryContract.GetProductIdsByBrandIdAsync(brand.Id);

        brand.Edit(dto.Title);

        await _unitOfWorkContract.SaveAsync();

        await _cacheService.RemoveAsync(_cacheKeyBuilder.Brand(dto.Id));
        await _cacheService.RemoveAsync(_cacheKeyBuilder.BrandList());
        await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.BrandSearchPattern());

        foreach (var productId in productIds)
        {
            await _cacheService.RemoveAsync(_cacheKeyBuilder.Product(productId));
        }

        await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.ProductListPattern());

        _logger.LogInformation(
            LogEvents.ProductBrand.EditCompleted,
            "Product brand edited successfully. ProductBrandId: {ProductBrandId}",
            dto.Id);

        return "برند محصول با موفقیت تغییر کرد .";
    }

    public async Task<ViewProductBrandDto> SearchProductBrandByTitle(
        SearchProductBrandDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(SearchProductBrandByTitle));

        _logger.LogInformation(
            LogEvents.ProductBrand.SearchStarted,
            "Product brand search started.");

        var cacheKey = _cacheKeyBuilder.BrandSearch(dto.Title);
        var cachedBrandSearch = await _cacheService.GetAsync<ViewProductBrandDto>(cacheKey);

        if (cachedBrandSearch is not null)
        {
            activity?.SetTag("cache.hit", true);

            _logger.LogInformation(
                LogEvents.ProductBrand.SearchCompleted,
                "Product brand search retrieved successfully from cache. Count: {Count}",
                cachedBrandSearch.Items.Count
            );

            return cachedBrandSearch;
        }

        activity?.SetTag("cache.hit", false);

        var brands =
            await _productRepositoryContract
                .SearchProductBrandsWithTitle(dto.Title);

        if (brands is null)
        {
            _logger.LogInformation(
                LogEvents.ProductBrand.SearchCompleted,
                "Product brand search completed with no results.");

            var emptyResult = new ViewProductBrandDto
            {
                Items = []
            };

            await _cacheService.SetAsync(
                key: cacheKey,
                value: emptyResult,
                expiration: TimeSpan.FromMinutes(10)
            );

            return emptyResult;
        }

        var result = new ViewProductBrandDto
        {
            Items = brands
                .Select(x => new ViewProductBrandItemDto
                {
                    Title = x.Title
                })
                .ToList()
        };

        await _cacheService.SetAsync(
            key: cacheKey,
            value: result,
            expiration: TimeSpan.FromMinutes(10)
        );
        activity?.SetTag("product_brands.count", result.Items.Count);

        _logger.LogInformation(
            LogEvents.ProductBrand.SearchCompleted,
            "Product brand search completed. Count: {Count}",
            result.Items.Count);

        return result;
    }

    public async Task<ViewProductBrandItemDto> GetProductBrandById(
        Guid productBrandId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(GetProductBrandById));

        activity?.SetTag("product_brand.id", productBrandId);

        var cacheKey = _cacheKeyBuilder.Brand(productBrandId);
        var cachedBrand = await _cacheService.GetAsync<ViewProductBrandItemDto>(cacheKey);

        if (cachedBrand is not null)
        {
            activity?.SetTag("cache.hit", true);

            _logger.LogInformation(
                LogEvents.ProductBrand.GetByIdCompleted,
                "Product brand retrieved successfully from cache.ProductBrandId: {ProductBrandId}",
                productBrandId
            );

            return cachedBrand;
        }

        var brand =
            await _productRepositoryContract
                .GetProductBrandById(productBrandId);

        if (brand is null)
        {
            _logger.LogWarning(
                LogEvents.ProductBrand.NotFound,
                "Product brand was not found. ProductBrandId: {ProductBrandId}",
                productBrandId);

            throw new NotFoundException(
                "برند محصول یافت نشد");
        }

        _logger.LogInformation(
            LogEvents.ProductBrand.GetByIdCompleted,
            "Product brand retrieved successfully. ProductBrandId: {ProductBrandId}",
            productBrandId);

        var result = new ViewProductBrandItemDto
        {
            Title = brand.Title
        };

        await _cacheService.SetAsync(
            key: cacheKey,
            value: result,
            expiration: TimeSpan.FromMinutes(10)
        );

        return result;
    }

    #endregion

    #region Review

    public async Task<ViewReviewsDto> GetAllProductReviews(Guid productId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(GetAllProductReviews));

        activity?.SetTag("product.id", productId);

        var product =
            await _productRepositoryContract
                .GetProductByIdAsync(productId);

        if (product is null)
        {
            _logger.LogWarning(
                LogEvents.Review.ProductNotFound,
                "Reviews retrieval rejected because the product was not found. ProductId: {ProductId}",
                productId);

            throw new NotFoundException("محصول یافت نشد");
        }

        var cacheKey = _cacheKeyBuilder.ReviewList(productId);
        var cacheReview = await _cacheService.GetAsync<ViewReviewsDto>(cacheKey);

        if (cacheReview is not null)
        {
            activity?.SetTag("cache.hit", true);

            _logger.LogInformation(
                LogEvents.Review.GetAllCompleted,
                "Product reviews retrieved from cache. ProductId: {ProductId}, Count: {Count}",
                productId,
                cacheReview.Reviews.Count);

            return cacheReview;
        }

        activity?.SetTag("cache.hit", false);

        if (product.Reviews.Count == 0)
        {
            _logger.LogInformation(
                LogEvents.Review.GetAllCompleted,
                "Product reviews retrieved with no reviews. ProductId: {ProductId}",
                productId);
            
            var emptyResult = new ViewReviewsDto
            {
                Reviews = []
            };

            await _cacheService.SetAsync(
                key: cacheKey,
                value: emptyResult,
                expiration: TimeSpan.FromMinutes(5)
            );
            
            return emptyResult;
        }

        var result = new ViewReviewsDto
        {
            Reviews = product.Reviews
                .Select(r => new ViewReviewItemsDto
                {
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    StarsCount = r.StarsCount,
                    User = new ViewReviewItemUserDto
                    {
                        Name = r.User.FullName
                    }
                })
                .ToList()
        };

        await _cacheService.SetAsync(
            key: cacheKey,
            value: result,
            expiration: TimeSpan.FromMinutes(5)
        );

        activity?.SetTag("reviews.count", product.Reviews.Count);

        _logger.LogInformation(
            LogEvents.Review.GetAllCompleted,
            "Product reviews retrieved successfully. ProductId: {ProductId}, Count: {Count}",
            productId,
            product.Reviews.Count);

        return result;
    }

    public async Task<string> AddReviewForProduct(
        Guid productId,
        CreateReviewDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(AddReviewForProduct));

        activity?.SetTag("product.id", productId);

        var userId =
            _userContext.UserId
            ?? throw new UnauthorizedAccessException("کاربر یافت نشد");

        activity?.SetTag("user.id", userId);

        _logger.LogInformation(
            LogEvents.Review.CreateStarted,
            "Product review creation started. ProductId: {ProductId}",
            productId);

        var product =
            await _productRepositoryContract
                .GetProductByIdAsync(productId);

        if (product is null)
        {
            _logger.LogWarning(
                LogEvents.Review.ProductNotFound,
                "Review creation rejected because the product was not found. ProductId: {ProductId}",
                productId);

            throw new NotFoundException("محصول یافت نشد");
        }

        var isExistCommentByUser =
            await _reviewsRepositoryContract
                .ExistsByUserAndProductAsync(productId, userId);

        if (isExistCommentByUser)
        {
            _logger.LogWarning(
                LogEvents.Review.Duplicate,
                "Review creation rejected because the user has already reviewed the product. ProductId: {ProductId}, UserId: {UserId}",
                productId,
                userId);

            throw new ConflictException(
                "شما قبلا برای این محصول نظر ثبت کرده اید.");
        }

        var review = new Domain.Entities.Review(
            starsCount: dto.StarsCount,
            comment: dto.Comment,
            userId: userId,
            productId: productId);

        await _reviewsRepositoryContract.AddReview(review);
        await _unitOfWorkContract.SaveAsync();

        await _cacheService.RemoveAsync(_cacheKeyBuilder.ReviewList(productId));
        await _cacheService.RemoveAsync(_cacheKeyBuilder.Product(productId));
        await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.ProductListPattern());
        
        
        activity?.SetTag("review.id", review.Id);

        _logger.LogInformation(
            LogEvents.Review.CreateCompleted,
            "Product review created successfully. ReviewId: {ReviewId}, ProductId: {ProductId}",
            review.Id,
            productId);

        return "نظر شما با موفقیت ثبت شد";
    }

    #endregion

    #region Variant

    public async Task<string> EditProductVariantAsync(
        EditProductVariantDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(EditProductVariantAsync));

        activity?.SetTag("product_variant.id", dto.Id);

        if (dto.Sku is null &&
            dto.Price is null)
        {
            _logger.LogWarning(
                LogEvents.ProductVariant.EditRejected,
                "Product variant edit rejected because no field was provided. ProductVariantId: {ProductVariantId}",
                dto.Id);

            throw new BusinessException(
                "برای ویرایش Variant، حداقل یک فیلد را تغییر دهید.");
        }

        int attempt = 0;
        const int maxAttempts = 5;

        _logger.LogInformation(
            LogEvents.ProductVariant.EditStarted,
            "Product variant edit started. ProductVariantId: {ProductVariantId}",
            dto.Id);

        while (attempt < maxAttempts)
        {
            var variant =
                await _productRepositoryContract
                    .GetProductVariantByIdAsync(dto.Id);

            if (variant is null)
            {
                _logger.LogWarning(
                    LogEvents.ProductVariant.NotFound,
                    "Product variant was not found. ProductVariantId: {ProductVariantId}",
                    dto.Id);

                throw new NotFoundException(
                    "Variant یافت نشد!");
            }

            try
            {
                variant.Edit(
                    dto.Sku,
                    dto.Price);

                await _unitOfWorkContract.SaveAsync();
                
                await _cacheService.RemoveAsync(_cacheKeyBuilder.Product(variant.ProductId));
                await _cacheService.RemoveByPatternAsync(_cacheKeyBuilder.ProductListPattern());
                
                _logger.LogInformation(
                    LogEvents.ProductVariant.EditCompleted,
                    "Product variant edited successfully. ProductVariantId: {ProductVariantId}",
                    dto.Id);

                return "Variant با موفقیت تغییر کرد";
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;

                _unitOfWorkContract.ClearChangeTracker();

                _logger.LogWarning(
                    LogEvents.ProductVariant.ConcurrencyConflict,
                    "Product variant edit concurrency conflict. ProductVariantId: {ProductVariantId}, Attempt: {Attempt}, MaxAttempts: {MaxAttempts}",
                    dto.Id,
                    attempt,
                    maxAttempts);

                if (attempt == maxAttempts)
                {
                    _logger.LogError(
                        LogEvents.ProductVariant.Failed,
                        "Product variant edit failed after maximum concurrency retry attempts. ProductVariantId: {ProductVariantId}, Attempts: {Attempts}",
                        dto.Id,
                        attempt);

                    throw new ConflictException(
                        "Variant در حال تغییر است. لطفاً دوباره تلاش کنید.");
                }
            }
        }

        throw new InvalidOperationException(
            "خطای ناشناخته");
    }

    #endregion
}