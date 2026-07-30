namespace Application.Features.Product.DTOs;

public class CreateProductDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public long Price { get; set; }
    public int Quantity { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? BrandId { get; set; }
}