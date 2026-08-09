using Application.Features.Coupon.DTOs;
using Application.Features.Coupon.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly CouponsServiceContract _couponsServiceContract;

    public CouponsController(CouponsServiceContract couponsServiceContract)
    {
        _couponsServiceContract = couponsServiceContract;
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetAllCouponsForAdmin()
    {
        var coupons=await _couponsServiceContract.GetAllCouponsForAdminAsync();
        return Ok(coupons);
    }
    
    [HttpGet("{couponId}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetCouponByIdForAdmin([FromRoute]Guid couponId)
    {
        var coupons=await _couponsServiceContract.GetCouponByIdForAdminAsync(couponId);
        return Ok(coupons);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CreateCoupon([FromBody]CreateCouponDto dto)
    {
        var result = await _couponsServiceContract.CreateCouponAsync(dto);
        return Ok(new
        {
            message = result
        });
    }

    [HttpPut]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> EditCoupon([FromBody]EditCouponDto dto)
    {
        var result = await _couponsServiceContract.EditCouponAsync(dto);
        return Ok(new
        {
            message = result
        });
    }

    [HttpDelete("{couponId}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteCoupon([FromRoute]Guid couponId)
    {
        var result=await _couponsServiceContract.DeleteCouponAsync(couponId);
        return Ok(new
        {
            message = result
        });
    }
    [HttpDelete("{couponId}/restore")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> RestoreCoupon([FromRoute]Guid couponId)
    {
        var result=await _couponsServiceContract.RestoreCouponAsync(couponId);
        return Ok(new
        {
            message = result
        });
    }

    [HttpPatch("{couponId}/activate")]
    public async Task<IActionResult> ActivateCoupon([FromRoute]Guid couponId)
    {
        var result=await _couponsServiceContract.ActivateCouponAsync(couponId);
        return Ok(new
        {
            message = result
        });
    }
    
    [HttpPatch("{couponId}/deactivate")]
    public async Task<IActionResult> DeActivateCoupon([FromRoute]Guid couponId)
    {
        var result=await _couponsServiceContract.DeActivateCouponAsync(couponId);
        return Ok(new
        {
            message = result
        });
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCoupon([FromBody]ValidateCouponDto dto)
    {
        var result = await _couponsServiceContract.ValidateCouponAsync(dto);
        return Ok(result);
    }
}