using Application.Features.Checkout.DTOs;
using Application.Features.Checkout.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutsController : ControllerBase
{
    private readonly CheckoutServiceContract _checkoutServiceContract;

    public CheckoutsController(CheckoutServiceContract checkoutServiceContract)
    {
        _checkoutServiceContract = checkoutServiceContract;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Checkout([FromBody]CheckoutDto dto)
    {
        var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault()
                             ?? Guid.NewGuid().ToString();

        var orderId = await _checkoutServiceContract.CheckoutAsync(dto, idempotencyKey);
        return Ok(orderId);
    }
}