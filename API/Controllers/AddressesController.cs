using Application.Features.Address.DTOs;
using Application.Features.Address.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AddressesController:ControllerBase
{
    private readonly AddressServiceContract _addressServiceContract;

    public AddressesController(AddressServiceContract addressServiceContract)
    {
        _addressServiceContract = addressServiceContract;
    }

    [HttpGet]
    public async Task<IActionResult> GetAddresses()
    {
        var addresses =await _addressServiceContract.GetAddressAsync();
        return Ok(addresses);
    }

    [HttpGet("{addressId}")]
    public async Task<IActionResult> GetAddressById([FromRoute]Guid addressId)
    {
        var address=await _addressServiceContract.GetAddressByIdAsync(addressId);
        return Ok(address);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody]CreateAddressDto dto)
    {
        var result = await _addressServiceContract.CreateAddressAsync(dto);
        return Ok(new
        {
            message = result
        });
    }

    [HttpPatch("{addressId}")]
    public async Task<IActionResult> EditAddress([FromRoute]Guid addressId,[FromBody]EditAddressDto dto)
    {
        var result=await _addressServiceContract.EditAddressAsync(addressId,dto);
        return Ok(new
        {
            message = result
        });
    }

    [HttpDelete("{addressId}")]
    public async Task<IActionResult> DeleteAddress([FromRoute]Guid addressId)
    {
        var result=await _addressServiceContract.DeleteAddressByIdAsync(addressId);
        return Ok(new
        {
            message = result
        });
    }

    [HttpPut("{addressId}/default")]
    public async Task<IActionResult> SetAddressDefault([FromRoute]Guid addressId)
    {
        var result=await _addressServiceContract.SetAddressDefaultAsync(addressId);
        return Ok(new
        {
            message = result
        });
    }
}