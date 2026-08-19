using Application.Features.Discount.Interfaces;
using Application.Features.Product.DTOs;
using Application.Features.Product.Interfaces;
using Application.Features.Review.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductServicesContract _productServicesContract;
    private readonly DiscountServiceContract _discountServiceContract;
    public ProductsController(ProductServicesContract productServicesContract, DiscountServiceContract discountServiceContract)
    {
        _productServicesContract = productServicesContract;
        _discountServiceContract = discountServiceContract;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductQueryDto query)
    {
        // BUG : Sku null
        var product = await _productServicesContract.GetAllProducts(query);
        return Ok(product);
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetProduct(Guid productId)
    {
        var product = await _productServicesContract.GetProductById(productId);
        return Ok(product);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> AddProduct(CreateProductDto dto)
    {
        var result = await _productServicesContract.AddProductAsync(dto);
        return Ok(new
        {
            message = result
        });
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPatch]
    public async Task<IActionResult> EditProduct([FromBody] EditProductDto dto)
    {
        var result = await _productServicesContract.EditProductAsync(dto);
        return Ok(new
            {
                message = result
            }
        );
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid productId)
    {
        var result = await _productServicesContract.DeleteProductAsync(productId);
        return Ok(new
        {
            message = result
        });
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost("{productId:guid}/restore")]
    public async Task<IActionResult> RestoreProduct([FromRoute] Guid productId)
    {
        var result = await _productServicesContract.RestoreProductAsync(productId);
        return Ok(new
        {
            message = result
        });
    }

    [HttpGet]
    [Route("Search")]
    public async Task<IActionResult> SearchProduct([FromQuery] string query)
    {
        var product = await _productServicesContract.SearchProductByTitle(query);
        return Ok(product);
    }

    [HttpGet("{productId:guid}/Reviews")]
    public async Task<IActionResult> GetProductReviews(Guid productId)
    {
        var reviews = await _productServicesContract.GetAllProductReviews(productId);
        return Ok(reviews);
    }

    [HttpPost("{productId:guid}/Reviews")]
    [Authorize]
    public async Task<IActionResult> AddReviewForProduct([FromRoute] Guid productId, [FromBody] CreateReviewDto dto)
    {
        var result = await _productServicesContract.AddReviewForProduct(productId, dto);
        return Ok(new
        {
            message = result
        });
    }
    
    [HttpGet("{productId}/discounts")]
    public async Task<IActionResult> GetDiscountByProductId([FromRoute]Guid productId)
    {
        var discount=await _discountServiceContract.GetDiscountByProductId(productId);
        return Ok(discount);
    }
}