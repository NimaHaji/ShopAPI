namespace Application.Features.Product.DTOs;

public class EditProductVariantDto
{
    public Guid Id { get; set; }
    public string? Sku { get; set; }
    public long? Price { get; set; }
}