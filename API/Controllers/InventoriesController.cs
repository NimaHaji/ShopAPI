using Application.Features.Inventory.DTOs;
using Application.Features.Inventory.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Exceptions;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoriesController : ControllerBase
{
    private readonly InventoryServiceContract _inventoryServiceContract;

    public InventoriesController(InventoryServiceContract inventoryServiceContract)
    {
        _inventoryServiceContract = inventoryServiceContract;
    }

    [HttpGet]
    public async Task<ActionResult<List<ViewInventoryItemDto>>> GetAll()
    {
        var result = await _inventoryServiceContract.GetAllInventoryAsync();
        return Ok(result);
    }

    [HttpGet("{productId}")]
    public async Task<ActionResult<ViewInventoryItemDto>> GetByProductId(Guid productId)
    {
        var result = await _inventoryServiceContract.GetInventoryByProductIdAsync(productId);
        return Ok(result);
    }

    [HttpPost("reserve")]
    public async Task<ActionResult<ViewInventoryItemDto>> ReserveStock([FromBody] StockReserveRequestDto request)
    {
        try
        {
            var result = await _inventoryServiceContract.ReserveStockAsync(
                request.ProductId,
                request.Quantity,
                request.OrderReference);

            return Ok(result);
        }
        catch (ConflictException e)
        {
            return Conflict(new { message = e.Message });
        }
    }

    [HttpPost("confirm")]
    public async Task<ActionResult<ViewInventoryItemDto>> ConfirmReservation([FromBody] StockReserveRequestDto request)
    {
        var result = await _inventoryServiceContract.ConfirmReservationAsync(
            request.ProductId,
            request.Quantity,
            request.OrderReference);

        return Ok(result);
    }

    [HttpPost("cancel")]
    public async Task<ActionResult<ViewInventoryItemDto>> CancelReservation([FromBody] StockReserveRequestDto request)
    {
        var result = await _inventoryServiceContract.CancelReservationAsync(
            request.ProductId,
            request.Quantity,
            request.OrderReference);

        return Ok(result);
    }

    [HttpPost("add-stock")]
    public async Task<ActionResult<ViewInventoryItemDto>> AddStock([FromBody] StockAddRequestDto request)
    {
        var result = await _inventoryServiceContract.AddStockAsync(
            request.ProductId,
            request.Quantity,
            request.Description);

        return Ok(result);
    }
}