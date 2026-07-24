namespace Application.Features.Inventory.DTOs;

public class StockAddRequestDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string Description { get; set; }
}