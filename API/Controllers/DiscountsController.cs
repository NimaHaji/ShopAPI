using Application.Features.Discount.DTOs;
using Application.Features.Discount.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountsController : ControllerBase
{
    private readonly DiscountServiceContract _discountServiceContract;

    public DiscountsController(DiscountServiceContract discountServiceContract)
    {
        _discountServiceContract = discountServiceContract;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDiscount()
    {
        var discounts = await _discountServiceContract.GetAllDiscountsAsync();
        return Ok(discounts);
    }

    [HttpGet("{discountId}")]
    public async Task<IActionResult> GetDiscountById([FromRoute] Guid discountId)
    {
        var discount = await _discountServiceContract.GetDiscountByIdAsync(discountId);
        return Ok(discount);
    }

    [HttpPatch("{discountId}/activate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ActiveDiscount([FromRoute] Guid discountId)
    {
        var result = await _discountServiceContract.ActivateDiscountAsync(discountId);
        return Ok(new
        {
            message = result
        });
    }

    [HttpPatch("{discountId}/deactivate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeActiveDiscount([FromRoute] Guid discountId)
    {
        var result = await _discountServiceContract.DeActivateDiscountAsync(discountId);
        return Ok(new
        {
            message = result
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CreateDiscount([FromBody] CreateDiscountDto dto)
    {
        var result = await _discountServiceContract.CreateDiscountAsync(dto);
        return Ok(new
        {
            message = result
        });
    }

    [HttpPut("{discountId}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> EditDiscount([FromRoute] Guid discountId, [FromBody] EditDiscountDto dto)
    {
        var result = await _discountServiceContract.EditDiscountByIdAsync(discountId, dto);
        return Ok(new
        {
            message = result
        });
    }

    [HttpDelete("{discountId}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteDiscount([FromRoute] Guid discountId)
    {
        var result = await _discountServiceContract.DeleteDiscountByIdAsync(discountId);
        return Ok(new
        {
            message = result
        });
    }
    
    [HttpPost("{discountId}/restore")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> RestoreDiscount([FromRoute] Guid discountId)
    {
        var result = await _discountServiceContract.RestoreDiscountByIdAsync(discountId);
        return Ok(new
        {
            message = result
        });
    }

    [HttpPost("{discountId}/products")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> SetDiscountForProducts([FromRoute]Guid discountId,[FromBody]AddProductToDiscountDto dto) 
    {
        var result = await _discountServiceContract.SetDiscountForProductAsync(discountId,dto);
        return Ok(new
        {
            message = result
        });
    }

    [HttpDelete("{discountId}/products/{productId}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteDiscountForProduct([FromRoute]Guid discountId,[FromRoute]Guid productId)
    {
        var result=await _discountServiceContract.DeleteDiscountFoProduct(discountId,productId);
        return Ok(new
        {
            message = result
        });
    }

    [HttpGet("{productId}/products")]
    public async Task<IActionResult> GetDiscountByProductId([FromRoute]Guid productId)
    {
        var discount=await _discountServiceContract.GetDiscountByProductId(productId);
        return Ok(discount);
    }
    
}