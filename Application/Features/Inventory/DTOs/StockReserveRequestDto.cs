namespace Application.Features.Inventory.DTOs;

public class StockReserveRequestDto
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public string OrderReference { get; set; }
}