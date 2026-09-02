namespace Application.Features.Inventory.DTOs;

public class StockAddRequestDto
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public string Description { get; set; }
}