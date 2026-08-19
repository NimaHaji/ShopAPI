namespace Application.Features.Inventory.DTOs;

public class ViewInventoryItemDto
{
    public Guid InventoryId { get; set; }

    public Guid ProductVariantId { get; set; }

    public Guid ProductId { get; set; }

    public string ProductTitle { get; set; }

    public string VariantSku { get; set; }

    public long Price { get; set; }

    public int StockQuantity { get; set; }

    public int ReservedQuantity { get; set; }

    public int AvailableQuantity { get; set; }

    public DateTime LastUpdated { get; set; }

    public List<ViewInventoryVariantOptionDto> Options { get; set; } = new();

    public List<ViewTransactionDto> RecentTransactions { get; set; } = new();
}

public class ViewInventoryVariantOptionDto
{
    public string Name { get; set; }

    public string Value { get; set; }
}