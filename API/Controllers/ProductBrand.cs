using Application.Features.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ProductBrand : Controller
{
    private readonly ProductServicesContract _productServicesContract;

    public ProductBrand(ProductServicesContract productServicesContract)
    {
        _productServicesContract = productServicesContract;
    }
}