using Application.Features.Cart.DTOs;
using Application.Features.Cart.implementations;
using Application.Features.Cart.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartsController : ControllerBase
{
    private readonly CartServicesContract _cartService;

    public CartsController(CartServicesContract cartService)
    {
        _cartService = cartService;
    }

    [HttpGet("Cart")]
    [Authorize]
    public async Task<IActionResult> GetCartAsync()
    {
        var cartView = await _cartService.GetCartByUserIdAsync();
        return Ok(cartView);
    }

    [HttpPost("items")]
    [Authorize]
    public async Task<IActionResult> AddItem(AddCartItemDto item)
    {
        var result = await _cartService.AddItemAsync(item);
        return Ok(result);
    }

    [HttpPut("items")]
    [Authorize]
    public async Task<IActionResult> UpdateQuantity(UpdateCartDto dto)
    {
        var result = await _cartService.UpdateItemQuantityAsync(dto);
        return Ok(new
        {
            message = result
        });
    }

    [HttpDelete("items/{productId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteItem(Guid productId)
    {
        var result = await _cartService.DeleteItemAsync(productId);
        return Ok(new
        {
            message = result
        });
    }

    [HttpDelete("clear")]
    [Authorize]
    public async Task<IActionResult> ClearCart()
    {
        var result = await _cartService.ClearCartAsync();
        return Ok(new
        {
            message =result
        });
    }

    [HttpGet("count")]
    [Authorize]
    public async Task<IActionResult> GetCount()
    {
        var count = await _cartService.GetCartItemsCountAsync();
        return Ok(new
        {
            itemscount = count
        });
    }
}