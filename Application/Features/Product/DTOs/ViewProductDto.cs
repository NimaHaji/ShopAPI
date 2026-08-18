using Domain.Enums;

namespace Application.Features.Product.DTOs;

public class ViewProductDto
{
    public List<ViewProductItemDto> Items { get; set; } = new();
}

public class ViewProductItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public long MinPrice { get; set; }
    public long MaxPrice { get; set; }
    public long FinalMinPrice { get; set; }
    public long FinalMaxPrice { get; set; }
    public int Stock { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public string Category { get; set; }
    public string? Brand { get; set; }
    public List<ViewProductImageDto> Images { get; set; } = new();
    public List<ViewProductOptionDto> Options { get; set; } = new();
    public List<ViewProductVariantDto> Variants { get; set; } = new();
}

public class ViewProductImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}

public class ViewProductOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<ViewProductOptionValueDto> Values { get; set; } = new();
}

public class ViewProductOptionValueDto
{
    public Guid Id { get; set; }
    public string Value { get; set; }
}

public class ViewProductVariantDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; }
    public long Price { get; set; }
    public long FinalPrice { get; set; }
    public DiscountType? DiscountType { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public long? DiscountAmount { get; set; }
    public int Stock { get; set; }
    public List<ViewProductVariantOptionDto> Options { get; set; } = new();
    public List<ViewProductImageDto> Images { get; set; } = new();
}

public class ViewProductVariantOptionDto
{
    public Guid Id { get; set; }
    public Guid ProductOptionId { get; set; }
    public string OptionName { get; set; }
    public Guid ProductOptionValueId { get; set; }
    public string Value { get; set; }
}