using Application.Features.Home.DTOs;
using Application.Features.Home.Interfaces;
using Application.Features.Product.Interfaces;
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
                var discount = CalculateDiscount(p, now);
                return new HomeProductOffersDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Price = p.Price,
                    FinalPrice = discount.FinalPrice,
                    DiscountType = discount.DiscountType,
                    DiscountPercentage = discount.DiscountPercentage,
                    DiscountAmount = discount.DiscountAmount,
                    ProductImage = p.Images.Select(x => x.ImageLink).FirstOrDefault()
                };
            }).ToList(),
            
            NewestProducts = newestProducts.Select(p =>
            {
                var discount = CalculateDiscount(p, now);
                return new HomeNewestProducts
                {
                    Id = p.Id,
                    Title = p.Title,
                    Price = p.Price,
                    FinalPrice = discount.FinalPrice,
                    DiscountType = discount.DiscountType,
                    DiscountPercentage = discount.DiscountPercentage,
                    DiscountAmount = discount.DiscountAmount,
                    ProductImage = p.Images.Select(x => x.ImageLink).FirstOrDefault()
                };
            }).ToList()
        };
    }

    private static (
        long FinalPrice,
        string? DiscountType,
        decimal? DiscountPercentage,
        long? DiscountAmount
        ) CalculateDiscount(Domain.Entities.Product product, DateTime now)
    {
        var activeDiscount = product.DiscountProducts
            .Select(dp => dp.Discount)
            .FirstOrDefault(d =>
                !d.IsDeleted &&
                d.IsActive &&
                d.StartsAt <= now &&
                d.EndsAt > now);

        long finalPrice = product.Price;
        long? discountAmount = null;
        decimal? discountPercentage = null;
        DiscountType? discountType = null;

        if (activeDiscount is not null)
        {
            discountType = activeDiscount.DiscountType;

            if (activeDiscount.DiscountType == DiscountType.Percentage)
            {
                discountPercentage = activeDiscount.Value;

                var calculatedDiscount =
                    product.Price * (activeDiscount.Value / 100);

                if (activeDiscount.MaxDiscountAmount.HasValue)
                {
                    calculatedDiscount = Math.Min(
                        calculatedDiscount,
                        activeDiscount.MaxDiscountAmount.Value);
                }

                discountAmount = (long)calculatedDiscount;

                finalPrice = product.Price - discountAmount.Value;
            }
            else if (activeDiscount.DiscountType == DiscountType.FixedAmount)
            {
                discountAmount = (long)activeDiscount.Value;

                finalPrice = product.Price - discountAmount.Value;
            }

            finalPrice = Math.Max(finalPrice, 0);
        }

        return (
            finalPrice,
            discountType?.ToString(),
            discountPercentage,
            discountAmount
        );
    }
}