using Domain.Enums;

namespace Application.Features.Product.DTOs;

public class ProductQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Q { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public long? MinPrice { get; set; }
    public long? MaxPrice { get; set; }
    public SortByType? SortBy { get; set; }
}