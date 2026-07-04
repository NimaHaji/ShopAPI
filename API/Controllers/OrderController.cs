using Application.Features.Order.DTOs;
using Application.Features.Order.Interfaces;
using Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class OrderController:ControllerBase
{
    private readonly OrderServicesContract _orderServicesContract;

    public OrderController(OrderServicesContract orderServicesContract)
    {
        _orderServicesContract = orderServicesContract;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody]CreateOrderDto dto)
    {
        var result = await _orderServicesContract.CreateOrderAsync(dto);
        return Ok(result);
    }
}