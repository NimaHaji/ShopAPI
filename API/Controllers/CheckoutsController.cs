using Application.Features.Checkout.DTOs;
using Application.Features.Checkout.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
    [EnableRateLimiting("Sensitive")]
    public async Task<IActionResult> Checkout(
        [FromBody] CheckoutDto dto,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
    {
        var orderId = await _checkoutServiceContract.CheckoutAsync(
            dto,
            idempotencyKey);

        return Ok(orderId);
    }
}