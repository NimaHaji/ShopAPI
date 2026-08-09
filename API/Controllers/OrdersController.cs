using Application.Features.Order.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderServicesContract _orderServicesContract;

    public OrdersController(OrderServicesContract orderServicesContract)
    {
        _orderServicesContract = orderServicesContract;
    }

    [HttpGet]
    [Route("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ViewOrdersForAdmin()
    {
        var orders = await _orderServicesContract.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ViewOrdersForUsers()
    {
        var orders = await _orderServicesContract.GetAllUserOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("{orderId}")]
    [Authorize]
    public async Task<IActionResult> GetOrderById([FromRoute] Guid orderId)
    {
        var order = await _orderServicesContract.GetOrderByIdAsync(orderId);
        return Ok(order);
    }

    [HttpPost("{orderId}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(Guid orderId)
    {
        var result=await _orderServicesContract.CancelOrderAsync(orderId);
        return Ok(new
        {
            message=result
        });
    }

    [HttpPatch("{orderId}/{status}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ChangeOrderStatus([FromRoute]Guid orderId, [FromRoute] OrderStatus status)
    {
        var result=await _orderServicesContract.ChangOrderStatusByIdAsync(orderId, status);
        return Ok(new
        {
            message=result
        });
    }
}