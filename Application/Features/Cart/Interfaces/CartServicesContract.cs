using Application.Features.Cart.DTOs;

namespace Application.Features.Cart.Interfaces;

public interface CartServicesContract
{
    Task<string> AddItemAsync(AddCartItemDto dto);
    Task<string> UpdateItemQuantityAsync(UpdateCartDto dto);
    Task<ViewCartDto> GetCartByUserIdAsync();
    Task<string> DeleteItemAsync(Guid productId);
    Task<string> ClearCartAsync();
    Task<int> GetCartItemsCountAsync();
}