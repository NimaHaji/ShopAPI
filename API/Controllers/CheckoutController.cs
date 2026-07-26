using Application.Features.Checkout.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly CheckoutServiceContract _checkoutServiceContract;

    public CheckoutController(CheckoutServiceContract checkoutServiceContract)
    {
        _checkoutServiceContract = checkoutServiceContract;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Checkout()
    {
        var orderId = await _checkoutServiceContract.CheckoutAsync();
        return Ok(orderId);
    }
}