namespace Application.Features.Cart.DTOs;

public class AddCartItemDto
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
}