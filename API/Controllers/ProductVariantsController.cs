using Application.Features.Discount.Interfaces;
using Application.Features.Product.DTOs;
using Application.Features.Product.implementations;
using Application.Features.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ProductVariants : ControllerBase
{
    private readonly DiscountServiceContract _discountServiceContract;
    private readonly ProductServicesContract _productServicesContract;
    
    public ProductVariants(DiscountServiceContract discountServiceContract, ProductServicesContract productServicesContract)
    {
        _discountServiceContract = discountServiceContract;
        _productServicesContract = productServicesContract;
    }

    [HttpGet("{productVariantId}/discounts")]
    public async Task<IActionResult> GetDiscountByProductVariantId([FromRoute]Guid productVariantId)
    {
        var discount=await _discountServiceContract.GetDiscountByProductVariantId(productVariantId);
        return Ok(discount);
    }

    [HttpPut]
    public async Task<IActionResult> EdtProductVariant(EditProductVariantDto dto)
    {
        var result = await _productServicesContract.EditProductVariantAsync(dto);
        return Ok();
    }
}