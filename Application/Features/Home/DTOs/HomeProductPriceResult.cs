using Domain.Enums;

namespace Application.Features.Home.DTOs;

public class HomeProductPriceResult
{
    public long MinPrice { get; set; }
    public long MaxPrice { get; set; }

    public long FinalMinPrice { get; set; }
    public long FinalMaxPrice { get; set; }

    public DiscountType? DiscountType { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public long? DiscountAmount { get; set; }
}

public class HomeVariantPriceResult
{
    public long Price { get; set; }
    public long FinalPrice { get; set; }

    public DiscountType? DiscountType { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public long? DiscountAmount { get; set; }
}