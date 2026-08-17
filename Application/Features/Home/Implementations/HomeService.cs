using Application.Features.Home.DTOs;
using Application.Features.Home.Interfaces;
using Application.Features.Product.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Home.Implementations;

public class HomeService : HomeServiceContract
{
    private readonly ProductRepositoryContract _productRepositoryContract;

    public HomeService(ProductRepositoryContract productRepositoryContract)
    {
        _productRepositoryContract = productRepositoryContract;
    }


    public async Task<HomeDto> GetHomeAsync()
    {
        var now = DateTime.UtcNow;

        var categories = await _productRepositoryContract.GetAllProductCategories();

        var productOffers = await _productRepositoryContract.GetDiscountedProducts();

        var brands = await _productRepositoryContract.GetAllBrandAsync();

        var newestProducts = await _productRepositoryContract.GetNewestProducts();

        return new HomeDto
        {
            Categories = categories.Select(x => new HomeCategoriesDto
                {
                    Id = x.Id,
                    CategoryName = x.Title
                })
                .ToList(),

            Brands = brands.Select(x => new HomeBrandsDto
            {
                Id = x.Id,
                BrandName = x.Title
            }).ToList(),

            ProductOffers = productOffers.Select(p =>
            {
                var price = CalculateProductPrice(p, now);

                return new HomeProductOffersDto
                {
                    Id = p.Id,
                    Title = p.Title,

                    MinPrice = price.MinPrice,
                    MaxPrice = price.MaxPrice,

                    FinalMinPrice = price.FinalMinPrice,
                    FinalMaxPrice = price.FinalMaxPrice,
                    
                    DiscountType = price.DiscountType,
                    DiscountPercentage = price.DiscountPercentage,
                    DiscountAmount = price.DiscountAmount,

                    ProductImage = p.Images
                        .OrderBy(x => x.SortOrder)
                        .Select(x => x.ImageLink)
                        .FirstOrDefault()
                };
            }).ToList(),

            NewestProducts = newestProducts.Select(p =>
            {
                var price = CalculateProductPrice(p, now);

                return new HomeNewestProducts
                {
                    Id = p.Id,
                    Title = p.Title,

                    MinPrice = price.MinPrice,
                    MaxPrice = price.MaxPrice,

                    FinalMinPrice = price.FinalMinPrice,
                    FinalMaxPrice = price.FinalMaxPrice,

                    DiscountType = price.DiscountType.ToString(),
                    DiscountPercentage = price.DiscountPercentage,
                    DiscountAmount = price.DiscountAmount,

                    ProductImage = p.Images
                        .OrderBy(x => x.SortOrder)
                        .Select(x => x.ImageLink)
                        .FirstOrDefault()
                };
            }).ToList()
        };
    }

    private static HomeProductPriceResult CalculateProductPrice(
        Domain.Entities.Product product,
        DateTime now)
    {
        var variants = product.Variants?
                           .Where(v => !v.IsDeleted)
                           .ToList()
                       ?? new List<ProductVariant>();

        if (variants.Count == 0)
        {
            return new HomeProductPriceResult
            {
                MinPrice = 0,
                MaxPrice = 0,
                FinalMinPrice = 0,
                FinalMaxPrice = 0
            };
        }

        var variantPrices = variants
            .Select(v =>
            {
                var discount = v.DiscountVariants?
                    .Where(dv => dv.Discount != null)
                    .Select(dv => dv.Discount!)
                    .FirstOrDefault(d =>
                        !d.IsDeleted &&
                        d.IsActive &&
                        d.StartsAt <= now &&
                        d.EndsAt > now);

                long finalPrice = v.Price;
                long? discountAmount = null;
                decimal? discountPercentage = null;
                DiscountType? discountType = null;


                if (discount != null)
                {
                    discountType = discount.DiscountType;

                    if (discount.DiscountType == DiscountType.Percentage)
                    {
                        discountPercentage = discount.Value;

                        var calculated =
                            v.Price * (discount.Value / 100m);

                        if (discount.MaxDiscountAmount.HasValue)
                        {
                            calculated = Math.Min(
                                calculated,
                                discount.MaxDiscountAmount.Value);
                        }

                        discountAmount = (long)calculated;

                        finalPrice = Math.Max(
                            0,
                            v.Price - discountAmount.Value);
                    }
                    else if (discount.DiscountType == DiscountType.FixedAmount)
                    {
                        discountAmount = Math.Min(
                            (long)discount.Value,
                            v.Price);

                        finalPrice =
                            v.Price - discountAmount.Value;
                    }
                }


                return new HomeVariantPriceResult
                {
                    Price = v.Price,
                    FinalPrice = finalPrice,
                    DiscountType = discountType,
                    DiscountPercentage = discountPercentage,
                    DiscountAmount = discountAmount
                };
            })
            .ToList();


        if (variantPrices.Count == 0)
        {
            return new HomeProductPriceResult
            {
                MinPrice = 0,
                MaxPrice = 0,
                FinalMinPrice = 0,
                FinalMaxPrice = 0
            };
        }


        var cheapest = variantPrices
            .OrderBy(x => x.FinalPrice)
            .First();


        return new HomeProductPriceResult
        {
            MinPrice = variantPrices.Min(x => x.Price),
            MaxPrice = variantPrices.Max(x => x.Price),

            FinalMinPrice = variantPrices.Min(x => x.FinalPrice),
            FinalMaxPrice = variantPrices.Max(x => x.FinalPrice),

            DiscountType = cheapest.DiscountType,
            DiscountPercentage = cheapest.DiscountPercentage,
            DiscountAmount = cheapest.DiscountAmount
        };
    }
}