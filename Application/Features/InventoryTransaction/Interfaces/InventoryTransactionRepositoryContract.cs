namespace Application.Features.InventoryTransaction.Interfaces;

public interface InventoryTransactionRepositoryContract
{
    Task AddInventoryTransactionAsync(Domain.Entities.InventoryTransaction inventoryTransaction);
}