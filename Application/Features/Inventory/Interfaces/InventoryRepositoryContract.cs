using Domain.Entities;

namespace Application.Features.Inventory.Interfaces;

public interface InventoryRepositoryContract
{
    Task<InventoryItem?> GetByProductVariantIdAsync(Guid productVariantId);
    Task<InventoryItem?> GetByIdAsync(Guid id);
    Task<List<InventoryItem>?> GetAllAsync();
    void UpdateAsync(InventoryItem  inventoryItem);
    Task AddAsync(InventoryItem inventory);
    Task<InventoryItem?> GetByProductVariantId(Guid productVariantId);
    Task<List<InventoryItem>?> GetByProductVariantIdsAsync(List<Guid> productVariantIds);
}