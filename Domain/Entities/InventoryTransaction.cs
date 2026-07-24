namespace Domain.Entities;

public class InventoryTransaction
{
    public Guid InventoryTransactionId { get; set; }
    public Guid InventoryItemId { get; set; }
    public TransactionType Type { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
        
    public InventoryItem InventoryItem { get; set; }
    private InventoryTransaction()
    {
        
    }
    public InventoryTransaction(Guid inventoryItemId,TransactionType transactionType,int quantity, string? reference, string? description)
    {
        InventoryTransactionId = Guid.NewGuid();
        InventoryItemId = inventoryItemId;
        Type = transactionType;
        Quantity = quantity;
        Reference = reference;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }
}