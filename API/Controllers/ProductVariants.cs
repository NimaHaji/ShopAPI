using Application.Features.Discount.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ProductVariants : ControllerBase
{
    private readonly DiscountServiceContract _discountServiceContract;

    public ProductVariants(DiscountServiceContract discountServiceContract)
    {
        _discountServiceContract = discountServiceContract;
    }

    [HttpGet("{productVariantId}/discounts")]
    public async Task<IActionResult> GetDiscountByProductVariantId([FromRoute]Guid productVariantId)
    {
        var discount=await _discountServiceContract.GetDiscountByProductVariantId(productVariantId);
        return Ok(discount);
    }
}