namespace Application.Features.Inventory.DTOs;

public class StockReserveRequestDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string OrderReference { get; set; }
}