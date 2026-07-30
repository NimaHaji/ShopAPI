namespace Application.Features.Inventory.DTOs;

public class ViewTransactionDto
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public int Quantity { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}