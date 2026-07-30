namespace Application.Features.Product.DTOs;

public class EditProductDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public long? Price { get; set; }
    public decimal? DiscountPercentage { get; set; }
}