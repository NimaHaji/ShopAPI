using Application.Features.Cart.DTOs;

namespace Application.Features.Cart.Interfaces;

public interface CartServicesContract
{
    Task<string> AddItemAsync(Guid userId,AddCartItemDto dto);
    Task UpdateItemQuantityAsync(Guid userId,UpdateCartDto dto);
    Task<ViewCartDto> GetCartByUserIdAsync(Guid userId);
    Task DeleteItemAsync(Guid userId, Guid productId);
    Task ClearCartAsync(Guid userId);
    Task<int> GetCartItemsCountAsync(Guid userId);
}