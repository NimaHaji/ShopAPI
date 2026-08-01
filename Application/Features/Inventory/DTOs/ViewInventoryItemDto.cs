namespace Application.Features.Inventory.DTOs;

public class ViewInventoryItemDto
{
    public Guid InventoryId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int StockQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<ViewTransactionDto> RecentTransactions { get; set; }
}