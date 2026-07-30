using Application.Features.Inventory.DTOs;
using Domain.Entities;

namespace Application.Features.Inventory.Interfaces;

public interface InventoryServiceContract
{
    public Task<List<ViewInventoryItemDto>> GetAllInventoryAsync();
    public Task<ViewInventoryItemDto> GetInventoryByProductIdAsync(Guid productId);
    public Task<ViewInventoryItemDto> ReserveStockAsync(Guid productId, int quantity, string orderReference);
    public Task ReserveAllItemStockAsync(List<CartItem> items);
    public Task<ViewInventoryItemDto> ConfirmReservationAsync(Guid productId, int quantity, string orderReference);
    public Task<ViewInventoryItemDto> CancelReservationAsync(Guid productId, int quantity, string orderReference);
    public Task<ViewInventoryItemDto> AddStockAsync(Guid productId, int quantity, string description);
    
}