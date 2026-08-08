using Application.Features.Coupon.DTOs;

namespace Application.Features.Coupon.Interfaces;

public interface CouponsServiceContract
{
    Task<ViewCouponDto> GetAllCouponsForAdminAsync();
    Task<ViewCouponitemsDto?> GetCouponByIdForAdminAsync(Guid couponId);
    Task<string> CreateCouponAsync(CreateCouponDto dto);
    Task<string> EditCouponAsync(EditCouponDto dto);
    Task<string> DeleteCouponAsync(Guid couponId);
    Task<string> RestoreCouponAsync(Guid couponId);
    Task<string> ActivateCouponAsync(Guid couponId);
    Task<string> DeActivateCouponAsync(Guid couponId);
    Task<ValidateCouponResponseDto> ValidateCouponAsync(ValidateCouponDto dto);
}