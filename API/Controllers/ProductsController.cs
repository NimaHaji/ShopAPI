using Application.Features.Auth.Interfaces;
using Application.Features.Product.DTOs;
using Application.Features.Product.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductServicesContract _productServicesContract;

    public ProductsController(ProductServicesContract productServicesContract)
    {
        _productServicesContract = productServicesContract;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery]ProductQueryDto query)
    {
        // Todo : fluent for query
        var product = await _productServicesContract.GetAllProducts(query);
        return Ok(product);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> AddProduct(CreateProductDto dto)
    {
        var result = await _productServicesContract.AddProductAsync(dto);
        return Ok(new
        {
            message = result
        });
    }

    [HttpGet("Category")]
    public async Task<IActionResult> GetProductCategories()
    {
        // Todo : Get product
        var categories = await _productServicesContract.GetAllCategories();
        return Ok(categories);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("Category")]
    public async Task<IActionResult> CreateProductCategory(CreateProductCategoryDto dto)
    {
        var res = await _productServicesContract.CreateProductCategory(dto);
        return Ok(new
        {
            message = res
        });
    }

    [HttpGet]
    [Route("Search")]
    public async Task<IActionResult> SearchProduct([FromQuery] string query)
    {
        var product = await _productServicesContract.SearchProductByTitle(query);
        return Ok(product);
    }
}