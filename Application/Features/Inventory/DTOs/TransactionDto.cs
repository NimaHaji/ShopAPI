namespace Application.Features.Inventory.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public int Quantity { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}