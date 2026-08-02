namespace Application.Features.Product.DTOs;

public class ViewProductDto
{
    public List<ViewProductItemDto> Items { get; set; }
}

public class ViewProductItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public long Price { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public int Stock { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public string Category { get; set; }
    public string? Brand { get; set; }
    public string Sku { get; set; }
    public List<string> Images { get; set; }
}