using Application.Features.Inventory.DTOs;

namespace Application.Features.Inventory.Interfaces;

public interface InventoryServiceContract
{
    public Task<List<ViewInventoryItemDto>> GetAllInventoryAsync();
    public Task<ViewInventoryItemDto> GetInventoryByProductVariantIdAsync(Guid productVariantId);
    public Task<ViewInventoryItemDto> ReserveStockAsync(Guid productVariantId, int quantity, string orderReference);
    public Task ReserveAllItemStockAsync(List<Domain.Entities.CartItem> items);
    public Task<ViewInventoryItemDto> ConfirmReservationAsync(Guid productVariantId, int quantity, string orderReference);
    public Task<ViewInventoryItemDto> CancelReservationAsync(Guid productVariantId, int quantity, string orderReference);
    public Task<ViewInventoryItemDto> AddStockAsync(Guid productVariantId, int quantity, string description);
    
}