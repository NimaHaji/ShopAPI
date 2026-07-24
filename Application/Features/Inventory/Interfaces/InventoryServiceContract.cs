using Application.Features.Inventory.DTOs;
using Domain.Entities;

namespace Application.Features.Inventory.Interfaces;

public interface InventoryServiceContract
{
    public Task<List<InventoryItemDto>> GetAllInventoryAsync();
    public Task<InventoryItemDto> GetInventoryByProductIdAsync(Guid productId);
    public Task<InventoryItemDto> ReserveStockAsync(Guid productId, int quantity, string orderReference);
    public Task ReserveAllItemStockAsync(List<CartItem> items);
    public Task<InventoryItemDto> ConfirmReservationAsync(Guid productId, int quantity, string orderReference);
    public Task<InventoryItemDto> CancelReservationAsync(Guid productId, int quantity, string orderReference);
    public Task<InventoryItemDto> AddStockAsync(Guid productId, int quantity, string description);
    
}