using Domain.Enums;

namespace Application.Features.Home.DTOs;

public class HomeDto
{
    public List<HomeCategoriesDto> Categories { get; set; } = null!;
    public List<HomeProductOffersDto> ProductOffers { get; set; } = null!;
    public List<HomeBrandsDto> Brands { get; set; } = null!;
    public List<HomeNewestProducts> NewestProducts { get; set; } = null!;
}

public class HomeNewestProducts
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public long MinPrice { get; set; }
    public long MaxPrice { get; set; }
    public long FinalMinPrice { get; set; }
    public long FinalMaxPrice { get; set; }
    public string? DiscountType { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public long? DiscountAmount { get; set; }
    public string? ProductImage { get; set; }
}

public class HomeBrandsDto
{
    public Guid Id { get; set; }
    public string BrandName { get; set; }= null!;
    // public string? BrandImage { get; set; }
}

public class HomeProductOffersDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public long MinPrice { get; set; }
    public long MaxPrice { get; set; }
    public long FinalMinPrice { get; set; }
    public long FinalMaxPrice { get; set; }
    public DiscountType? DiscountType { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public long? DiscountAmount { get; set; }
    public string? ProductImage { get; set; }
}

public class HomeCategoriesDto
{
    public Guid Id { get; set; }
    public string CategoryName { get; set; } = null!;
    // public string? ImageUrl { get; private set; }
}

public class HomeBannersDto
{
    public Guid Id { get; private set; }
    public string Url { get; private set; }
}