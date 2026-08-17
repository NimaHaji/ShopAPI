using Domain.Enums;
record PriceCalculationResult(long FinalPrice, long? DiscountAmount, decimal? DiscountPercentage, DiscountType? DiscountType);