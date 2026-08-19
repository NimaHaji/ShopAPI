using Domain.Enums;

namespace Application.Features.Product.DTOs;

record PriceCalculationResult(long FinalPrice, long? DiscountAmount, decimal? DiscountPercentage, DiscountType? DiscountType);