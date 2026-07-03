using Application.Features.Cart.DTOs;
using Application.Features.Cart.implementations;
using Application.Features.Cart.Interfaces;
using Domain.Entites;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly CartServicesContract _cartService;

    public CartController(CartServicesContract cartService)
    {
        _cartService = cartService;
    }

    [HttpGet("Cart")]
    public async Task<IActionResult> GetCartAsync([FromQuery] Guid userId)
    {
        var cartView = await _cartService.GetCartByUserIdAsync(userId);
        return Ok(cartView);
    }
    // Todo : userId From User jwt
    [HttpPost("items")]
    public async Task<IActionResult> AddItem(Guid userId, AddCartItemDto item)
    {
        var result = await _cartService.AddItemAsync(userId, item);
        return Ok(result);
    }
    
    [HttpPut("items")]
    public async Task<IActionResult> UpdateQuantity([FromQuery]Guid userId,UpdateCartDto dto)
    {
        await _cartService.UpdateItemQuantityAsync(userId, dto);
        return Ok(new { message = "تعداد با موفقیت بروزرسانی شد." });
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<IActionResult> DeleteItem([FromQuery]Guid userId ,Guid productId)
    {
        await _cartService.DeleteItemAsync(userId, productId);
        return Ok(new {message ="محصول با موفقیت حذف شد ."});
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart([FromQuery] Guid userId)
    {
        await _cartService.ClearCartAsync(userId);
        return Ok(new { message = "سبد خرید کاملاً خالی شد." });
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCount([FromQuery] Guid userId)
    {
        var count=await _cartService.GetCartItemsCountAsync(userId);
        return Ok(count);
    }
}