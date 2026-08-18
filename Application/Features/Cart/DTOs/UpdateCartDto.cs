namespace Application.Features.Cart.DTOs;

public class UpdateCartDto
{
    public Guid ProductVariantId { get; set; }
    public int NewQuantity { get; set; }
}