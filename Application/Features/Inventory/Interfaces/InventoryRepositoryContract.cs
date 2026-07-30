using Domain.Entities;

namespace Application.Features.Inventory.Interfaces;

public interface InventoryRepositoryContract
{
    Task<InventoryItem?> GetByProductIdAsync(Guid productId);
    Task<List<InventoryItem>?> GetByProductIdsAsync(List<Guid> productId);
    Task<InventoryItem?> GetByIdAsync(Guid id);
    Task<List<InventoryItem>?> GetAllAsync();
    void UpdateAsync(InventoryItem  inventoryItem);
    Task AddAsync(InventoryItem inventory);
    Task<InventoryItem?> GetByProductId(Guid productId);
}