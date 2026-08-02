using Application.Features.Product.DTOs;
using Application.Features.Product.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ProductServicesContract _productServicesContract;

    public CategoriesController(ProductServicesContract productServicesContract)
    {
        _productServicesContract = productServicesContract;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _productServicesContract.GetAllCategories();
        return Ok(categories);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> CreateProductCategory(CreateProductCategoryDto dto)
    {
        var res = await _productServicesContract.CreateProductCategoryAsync(dto);
        return Ok(new
        {
            message = res
        });
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpDelete("{categoryId:guid}")]
    public async Task<IActionResult> DeleteCategory([FromRoute] Guid categoryId)
    {
        var result = await _productServicesContract.DeleteProductCategoryAsync(categoryId);

        return Ok(new
        {
            message = result
        });
    }
    
    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost("{categoryId:guid}/restore")]
    public async Task<IActionResult> RestoreCategory([FromRoute] Guid categoryId)
    {
        var result = await _productServicesContract.RestoreProductCategoryAsync(categoryId);

        return Ok(new
        {
            message = result
        });
    }
    
    [HttpPut]
    public async Task<IActionResult> EditCategory(EditProductCategoryDto dto)
    {
        var result = await _productServicesContract.EditProductCategoryAsync(dto);
        return Ok(new
        {
            message = result
        });
    }

    [HttpGet]
    [Route("Search")]
    public async Task<IActionResult> SearchProductCategory([FromQuery]SearchProductCategoryDto dto)
    {
        var categories = await _productServicesContract.SearchProductCategoryByTitle(dto);
        return Ok(categories);
    }
    
    [HttpGet("{categoryId}")]
    public async Task<IActionResult> GetProduct(Guid categoryId)
    {
        var productCategory = await _productServicesContract.GetProductCategoryById(categoryId);
        return Ok(productCategory);
    }
}