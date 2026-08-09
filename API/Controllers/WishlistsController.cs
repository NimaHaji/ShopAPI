using Application.Features.Wishlist.DTOs;
using Application.Features.Wishlist.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistsController : ControllerBase
{
    private readonly WishlistServiceContract _wishlistServiceContract;

    public WishlistsController(WishlistServiceContract wishlistServiceContract)
    {
        _wishlistServiceContract = wishlistServiceContract;
    }

    [HttpGet]
    public async Task<IActionResult> GetWishlist()
    {
        var wishlist = await _wishlistServiceContract.GetWishlistAsync();
        return Ok(wishlist);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItemToWishlist([FromBody] AddWishlistItemDto dto)
    {
        var result = await _wishlistServiceContract.AddProductToWishlistAsync(dto);
        return Ok(new
        {
            message = result
        });
    }

    [HttpDelete("items/{productId}")]
    public async Task<IActionResult> DeleteWishlistItem([FromRoute] Guid productId)
    {
        var result = await _wishlistServiceContract.DeleteProductFromWishListAsync(productId);
        return Ok(new
        {
            message = result
        });
    }
    
    [HttpDelete("items")]
    public async Task<IActionResult> ClearWishlist()
    {
        var result = await _wishlistServiceContract.ClearWishListAsync();
        return Ok(new
        {
            message = result
        });
    }
    
    [HttpGet("count")]
    public async Task<IActionResult> GetCountOfWishListItem()
    {
        var count = await _wishlistServiceContract.GetWishlistItemsCountAsync();
        return Ok(new
        {
            WishlistItemsCount = count
        });
    }
}