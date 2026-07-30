using Application.Features.Product.DTOs;
using Application.Features.Product.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandController : ControllerBase
{
    private readonly ProductServicesContract _productServicesContract;

    public BrandController(ProductServicesContract productServicesContract)
    {
        _productServicesContract = productServicesContract;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBrands()
    {
        var brands = await _productServicesContract.GetAllProductBrands();
        return Ok(brands);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> CreateBrand(CreateProductBrandDto dto)
    {
        var res = await _productServicesContract.CreateProductBrandAsync(dto);
        return Ok(new
        {
            message = res
        });
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("{brandId:guid}")]
    public async Task<IActionResult> DeleteBrand([FromRoute] Guid brandId)
    {
        var result = await _productServicesContract.DeleteProductBrandAsync(brandId);

        return Ok(new
        {
            message = result
        });
    }

    [HttpPut]
    public async Task<IActionResult> EditBrand(EditProductBrandDto dto)
    {
        var result = await _productServicesContract.EditProductBrandAsync(dto);
        return Ok(new
        {
            message = result
        });
    }

    [HttpGet]
    [Route("Search")]
    public async Task<IActionResult> SearchProductBrand([FromQuery]SearchProductBrandDto dto)
    {
        var brands = await _productServicesContract.SearchProductBrandByTitle(dto);
        return Ok(brands);
    }
    
    [HttpGet("{brandId}")]
    public async Task<IActionResult> GetBrand(Guid brandId)
    {
        var productBrand = await _productServicesContract.GetProductBrandById(brandId);
        return Ok(productBrand);
    }
}