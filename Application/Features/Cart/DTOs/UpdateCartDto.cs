namespace Application.Features.Cart.DTOs;

public class UpdateCartDto
{
    public Guid ProductId { get; set; }
    public int NewQuantity { get; set; }
}