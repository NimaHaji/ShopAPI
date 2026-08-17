using System.Data;
using Application.Common.Interfaces;
using Application.Features.Inventory.Interfaces;
using Application.Features.Product.DTOs;
using Application.Features.Product.Interfaces;
using Application.Features.Review.DTOs;
using Application.Features.Review.interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Services;
using Microsoft.EntityFrameworkCore;
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

    public ProductService(ProductRepositoryContract productRepositoryContract,
        InventoryServiceContract inventoryServiceContract, UnitOfWorkContract unitOfWorkContract,
        SkuGeneratorContract skuGeneratorContract, ReviewsRepositoryContract reviewsRepositoryContract,
        IUSerContext userContext)
    {
        _productRepositoryContract = productRepositoryContract;
        _inventoryServiceContract = inventoryServiceContract;
        _unitOfWorkContract = unitOfWorkContract;
        _skuGeneratorContract = skuGeneratorContract;
        _reviewsRepositoryContract = reviewsRepositoryContract;
        _userContext = userContext;
    }

    #region product

    public async Task<ViewProductDto> GetAllProducts(ProductQueryDto query)
    {
        var products = await _productRepositoryContract.GetProductList(query);

        if (products is null)
        {
            return new ViewProductDto
            {
                Items = []
            };
        }

        var now = DateTime.UtcNow;

        var dto = products.Select(p =>
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
                })
                .ToList();

            var prices = variantDtos
                .Select(v => v.Price)
                .ToList();

            var finalPrices = variantDtos
                .Select(v => v.FinalPrice)
                .ToList();

            return new ViewProductItemDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Brand = p.Brand?.Title ?? "بدون برند",
                Category = p.Category.Title,
                
                MinPrice = prices.DefaultIfEmpty(0).Min(),
                MaxPrice = prices.DefaultIfEmpty(0).Max(),

                FinalMinPrice = finalPrices.DefaultIfEmpty(0).Min(),
                FinalMaxPrice = finalPrices.DefaultIfEmpty(0).Max(),

                
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
        }).ToList();

        return new ViewProductDto
        {
            Items = dto
        };
    }

    public async Task<string> AddProductAsync(CreateProductDto dto)
    {
        var isExist = await _productRepositoryContract.IsExistingProduct(dto.Title);
        if (isExist)
            throw new DuplicateNameException("این محصول از قبل وجود دارد");

        
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
        
        await _inventoryServiceContract.AddStockAsync(product.Id, dto.Quantity, product.Description);

        await _unitOfWorkContract.SaveAsync();

        return $"محصول {product.Title} ساخته شد";
    }

    public async Task<SearchProductResultDto> SearchProductByTitle(string query)
    {
        if (string.IsNullOrEmpty(query))
            return new SearchProductResultDto
            {
                Items = []
            };

        var productList = await _productRepositoryContract.SearchProductWithTitle(query);

        if (productList is null)
            return new SearchProductResultDto
            {
                Items = []
            };

        var dto = productList.Select(x => new SearchProductItemsResultDto
        {
            Title = x.Title,
            Category = x.Category.Title,
        }).ToList();


        return new SearchProductResultDto
        {
            Items = dto
        };
    }

    public async Task<ViewProductItemDto> GetProductById(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new BusinessException("شناسه محصول خالی است.");

        var product = await _productRepositoryContract.GetProductByIdAsync(productId);

        if (product is null)
            throw new NotFoundException("محصول یافت نشد.");

        var (rating, reviewCount) = await _reviewsRepositoryContract.GetProductRatingAsync(productId);

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
            })
            .ToList();
        
        var prices = variantDto
            .Select(v => v.Price)
            .ToList();

        var finalPrices = variantDto
            .Select(v => v.FinalPrice)
            .ToList();
        
        return new ViewProductItemDto
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
    }

    public async Task<string> EditProductAsync(EditProductDto dto)
    {
        if (dto.Title is null &&
            dto.Description is null)
            throw new BusinessException("برای ویرایش محصول، حداقل یک فیلد را تغییر دهید .");

        int attempt = 0;
        const int maxAttempts = 5;

        while (maxAttempts > attempt)
        {
            var product = await _productRepositoryContract.GetProductByIdAsync(dto.Id);

            if (product is null)
                throw new NotFoundException("محصولی یافت نشد !");

            try
            {
                product.Edit(dto.Title, dto.Description);

                await _unitOfWorkContract.SaveAsync();
                return "محصول با موفقیت تغییر کرد";
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;

                _unitOfWorkContract.ClearChangeTracker();

                if (attempt == maxAttempts)
                    throw new ConflictException("محصول در حال تغییر است . لطفا دوباره تلاش کنید");
            }
        }

        throw new InvalidOperationException("خطای ناشناخته");
    }

    public async Task<string> DeleteProductAsync(Guid productId)
    {
        int attempt = 0;
        const int maxAttempts = 5;

        while (maxAttempts > attempt)
        {
            var product = await _productRepositoryContract.GetProductByIdAsync(productId);

            if (product is null)
                throw new NotFoundException("محصولی یافت نشد !");

            try
            {
                product.Delete();

                await _unitOfWorkContract.SaveAsync();
                return "محصول با موفقیت حذف شد";
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;

                _unitOfWorkContract.ClearChangeTracker();

                if (attempt == maxAttempts)
                    throw new ConflictException("محصول در حال تغییر است . لطفا دوباره تلاش کنید");
            }
        }

        throw new InvalidOperationException("خطای ناشناخته");
    }

    public async Task<string> RestoreProductAsync(Guid productId)
    {
        int attempt = 0;
        const int maxAttempts = 5;

        while (maxAttempts > attempt)
        {
            var product = await _productRepositoryContract.GetProductByIdAsync(productId);

            if (product is null)
                throw new NotFoundException("محصولی یافت نشد !");

            try
            {
                product.Restore();

                await _unitOfWorkContract.SaveAsync();
                return "محصول با موفقیت بازیابی شد";
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;

                _unitOfWorkContract.ClearChangeTracker();

                if (attempt == maxAttempts)
                    throw new ConflictException("محصول در حال تغییر است . لطفا دوباره تلاش کنید");
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
        int attempt = 0;
        const int maxAttempts = 5;

        while (maxAttempts > attempt)
        {
            var productCategory = await _productRepositoryContract.GetProductCategoryById(productCategoryId);

            if (productCategory is null)
                throw new NotFoundException("دسته بندی محصولی یافت نشد !");

            try
            {
                productCategory.Delete();

                await _unitOfWorkContract.SaveAsync();
                return "دسته بندی محصول با موفقیت حذف شد";
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;

                _unitOfWorkContract.ClearChangeTracker();

                if (attempt == maxAttempts)
                    throw new ConflictException("دسته بندی محصول در حال تغییر است . لطفا دوباره تلاش کنید");
            }
        }

        throw new InvalidOperationException("خطای ناشناخته");
    }

    public async Task<string> RestoreProductCategoryAsync(Guid productCategoryId)
    {
        int attempt = 0;
        const int maxAttempts = 5;

        while (maxAttempts > attempt)
        {
            var productCategory = await _productRepositoryContract.GetProductCategoryById(productCategoryId);

            if (productCategory is null)
                throw new NotFoundException("دسته بندی محصولی یافت نشد !");

            try
            {
                productCategory.Restore();

                await _unitOfWorkContract.SaveAsync();
                return "دسته بندی محصول با موفقیت بازیابی شد";
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;

                _unitOfWorkContract.ClearChangeTracker();

                if (attempt == maxAttempts)
                    throw new ConflictException("دسته بندی محصول در حال تغییر است . لطفا دوباره تلاش کنید");
            }
        }

        throw new InvalidOperationException("خطای ناشناخته");
    }

    public async Task<ViewProductCategoryDto> GetAllCategories()
    {
        var categories = await _productRepositoryContract.GetAllProductCategories();

        var dto = categories.Select(x => new ViewProductCategoryItemDto
        {
            Title = x.Title
        }).ToList();

        return new ViewProductCategoryDto
        {
            Items = dto
        };
    }

    public async Task<ViewProductCategoryDto> SearchProductCategoryByTitle(SearchProductCategoryDto dto)
    {
        var categories = await _productRepositoryContract.SearchProductCategoriesWithTitle(dto.Title);

        if (categories is null)
            return new ViewProductCategoryDto()
            {
                Items = []
            };

        return new ViewProductCategoryDto
        {
            Items = categories.Select(x => new ViewProductCategoryItemDto
                {
                    Title = x.Title
                })
                .ToList()
        };
    }

    public async Task<ViewProductCategoryItemDto> GetProductCategoryById(Guid productCategoryId)
    {
        var category = await _productRepositoryContract.GetProductCategoryById(productCategoryId);

        if (category is null)
            throw new NotFoundException("دسته بندی محصول یافت نشد");

        var dto = new ViewProductCategoryItemDto
        {
            Title = category.Title
        };

        return dto;
    }

    public async Task<string> EditProductCategoryAsync(EditProductCategoryDto dto)
    {
        var category = await _productRepositoryContract.GetProductCategoryById(dto.Id);

        if (category is null)
            throw new NotFoundException("دسته بندی محصول یافت نشد .");

        category.Edit(dto.Title);

        await _unitOfWorkContract.SaveAsync();
        return "دسته بندی محصول یا موفقیت تغییر کرد .";
    }

    public async Task<string> CreateProductCategoryAsync(CreateProductCategoryDto dto)
    {
        var isExist = await _productRepositoryContract.IsExistingProductCategory(dto.Title);
        if (isExist)
            throw new DuplicateNameException("دسته بندی محصول وجود دارد");

        var category = ProductCategory.Create(dto.Title);

        await _productRepositoryContract.AddProductCategory(category);
        await _unitOfWorkContract.SaveAsync();

        return $"دسته بندی {category.Title} ساخته شد .";
    }

    #endregion

    #region Brand

    public async Task<ViewProductBrandDto> GetAllProductBrands()
    {
        var brands = await _productRepositoryContract.GetAllBrandAsync();

        if (brands is null)
            return new ViewProductBrandDto()
            {
                Items = []
            };

        return new ViewProductBrandDto()
        {
            Items = brands.Select(x => new ViewProductBrandItemDto()
                {
                    Title = x.Title
                })
                .ToList()
        };
    }

    public async Task<string> CreateProductBrandAsync(CreateProductBrandDto dto)
    {
        var isExist = await _productRepositoryContract.IsExistingBrand(dto.Title);
        if (isExist)
            throw new DuplicateNameException("برند محصول وجود دارد");

        var brand = ProductBrand.Create(dto.Title);

        await _productRepositoryContract.AddBrandAsync(brand);
        await _unitOfWorkContract.SaveAsync();

        return $"برند  {brand.Title} ساخته شد .";
    }

    public async Task<string> DeleteProductBrandAsync(Guid productBrandId)
    {
        int attempt = 0;
        const int maxAttempts = 5;

        while (maxAttempts > attempt)
        {
            var productCategory = await _productRepositoryContract.GetProductBrandById(productBrandId);

            if (productCategory is null)
                throw new NotFoundException("دسته بندی محصولی یافت نشد !");

            try
            {
                productCategory.Delete();

                await _unitOfWorkContract.SaveAsync();
                return "دسته بندی محصول با موفقیت حذف شد";
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;

                _unitOfWorkContract.ClearChangeTracker();

                if (attempt == maxAttempts)
                    throw new ConflictException("دسته بندی محصول در حال تغییر است . لطفا دوباره تلاش کنید");
            }
        }

        throw new InvalidOperationException("خطای ناشناخته");
    }

    public async Task<string> RestoreProductBrandAsync(Guid productBrandId)
    {
        int attempt = 0;
        const int maxAttempts = 5;

        while (maxAttempts > attempt)
        {
            var productBrand = await _productRepositoryContract.GetProductBrandById(productBrandId);

            if (productBrand is null)
                throw new NotFoundException("دسته بندی محصولی یافت نشد !");

            try
            {
                productBrand.Delete();

                await _unitOfWorkContract.SaveAsync();
                return "دسته بندی محصول با موفقیت حذف شد";
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;

                _unitOfWorkContract.ClearChangeTracker();

                if (attempt == maxAttempts)
                    throw new ConflictException("دسته بندی محصول در حال تغییر است . لطفا دوباره تلاش کنید");
            }
        }

        throw new InvalidOperationException("خطای ناشناخته");
    }

    public async Task<string> EditProductBrandAsync(EditProductBrandDto dto)
    {
        var brand = await _productRepositoryContract.GetProductBrandById(dto.Id);

        if (brand is null)
            throw new NotFoundException("برند محصول یافت نشد .");

        brand.Edit(dto.Title);

        await _unitOfWorkContract.SaveAsync();
        return "برند محصول یا موفقیت تغییر کرد .";
    }

    public async Task<ViewProductBrandDto> SearchProductBrandByTitle(SearchProductBrandDto dto)
    {
        var brands = await _productRepositoryContract.SearchProductBrandsWithTitle(dto.Title);

        if (brands is null)
            return new ViewProductBrandDto()
            {
                Items = []
            };

        return new ViewProductBrandDto()
        {
            Items = brands.Select(x => new ViewProductBrandItemDto()
                {
                    Title = x.Title
                })
                .ToList()
        };
    }

    public async Task<ViewProductBrandItemDto> GetProductBrandById(Guid productBrandId)
    {
        var brand = await _productRepositoryContract.GetProductBrandById(productBrandId);

        if (brand is null)
            throw new NotFoundException("دسته بندی محصول یافت نشد");

        var dto = new ViewProductBrandItemDto()
        {
            Title = brand.Title
        };

        return dto;
    }

    #endregion

    #region Review

    public async Task<ViewReviewsDto> GetAllProductReviews(Guid productId)
    {
        var product = await _productRepositoryContract.GetProductByIdAsync(productId);

        if (product is null)
            throw new NotFoundException("محصول یافت نشد");

        var reviews = await _reviewsRepositoryContract.GetAllReviewsByProductId(productId);

        if (reviews is null)
            return new ViewReviewsDto
            {
                Reviews = []
            };

        return new ViewReviewsDto
        {
            Reviews = product.Reviews.Select(r => new ViewReviewItemsDto
            {
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                StarsCount = r.StarsCount,
                User = new ViewReviewItemUserDto
                {
                    Name = r.User.FullName
                }
            }).ToList()
        };
    }

    public async Task<string> AddReviewForProduct(Guid productId, CreateReviewDto dto)
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر یافت نشد");

        var product = await _productRepositoryContract.GetProductByIdAsync(productId);

        if (product is null)
            throw new NotFoundException("محصول یافت نشد");

        var isExistCommentByUser = await _reviewsRepositoryContract.ExistsByUserAndProductAsync(productId, userId);

        if (isExistCommentByUser)
            throw new ConflictException("شما قبلا برای این محصول نظر ثبت کرده اید.");

        var review = new Domain.Entities.Review(
            starsCount: dto.StarsCount,
            comment: dto.Comment,
            userId: userId,
            productId: productId);

        await _reviewsRepositoryContract.AddReview(review);
        await _unitOfWorkContract.SaveAsync();

        return "نظر شما با موفقیت ثبت شد";
    }

    #endregion

    #region Variant

    public async Task<string> EditProductVariantAsync(EditProductVariantDto dto)
    {
        if (dto.Sku is null &&
            dto.Price is null)
        {
            throw new BusinessException(
                "برای ویرایش Variant، حداقل یک فیلد را تغییر دهید.");
        }

        int attempt = 0;
        const int maxAttempts = 5;

        while (attempt < maxAttempts)
        {
            var variant =
                await _productRepositoryContract.GetProductVariantByIdAsync(dto.Id);

            if (variant is null)
                throw new NotFoundException(
                    "Variant یافت نشد!");

            try
            {
                variant.Edit(
                    dto.Sku,
                    dto.Price);

                await _unitOfWorkContract.SaveAsync();

                return "Variant با موفقیت تغییر کرد";
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;

                _unitOfWorkContract.ClearChangeTracker();

                if (attempt == maxAttempts)
                {
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