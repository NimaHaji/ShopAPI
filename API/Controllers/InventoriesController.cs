using Application.Features.Inventory.DTOs;
using Application.Features.Inventory.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    public async Task<ActionResult> GetAll()
    {
        var result = await _inventoryServiceContract.GetAllInventoryAsync();

        return Ok(result);
    }

    [HttpGet("{productVariantId}")]
    public async Task<ActionResult> GetByProductVariantId(Guid productVariantId)
    {
        var result = await _inventoryServiceContract
            .GetInventoryByProductVariantIdAsync(productVariantId);

        return Ok(result);
    }

    [HttpPost("reserve")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [EnableRateLimiting("Sensitive")]
    public async Task<ActionResult<ViewInventoryItemDto>> ReserveStock(
        [FromBody] StockReserveRequestDto request)
    {
        try
        {
            var result = await _inventoryServiceContract.ReserveStockAsync(
                request.ProductVariantId,
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
    [Authorize(Roles = "Admin,SuperAdmin")]
    [EnableRateLimiting("Sensitive")]
    public async Task<ActionResult<ViewInventoryItemDto>> ConfirmReservation(
        [FromBody] StockReserveRequestDto request)
    {
        var result = await _inventoryServiceContract.ConfirmReservationAsync(
            request.ProductVariantId,
            request.Quantity,
            request.OrderReference);

        return Ok(result);
    }

    [HttpPost("cancel")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [EnableRateLimiting("Sensitive")]
    public async Task<ActionResult<ViewInventoryItemDto>> CancelReservation(
        [FromBody] StockReserveRequestDto request)
    {
        var result = await _inventoryServiceContract.CancelReservationAsync(
            request.ProductVariantId,
            request.Quantity,
            request.OrderReference);

        return Ok(result);
    }

    [HttpPost("add-stock")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [EnableRateLimiting("Write")]
    public async Task<ActionResult<ViewInventoryItemDto>> AddStock(
        [FromBody] StockAddRequestDto request)
    {
        var result = await _inventoryServiceContract.AddStockAsync(
            request.ProductVariantId,
            request.Quantity,
            request.Description);

        return Ok(result);
    }
}