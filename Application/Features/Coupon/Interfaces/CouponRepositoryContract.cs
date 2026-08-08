namespace Application.Features.Coupon.Interfaces;

public interface CouponRepositoryContract
{
    Task<List<Domain.Entities.Coupon>> GetAllDiscountsForAdminAsync();
    Task<Domain.Entities.Coupon?> GetCouponByIdForAdminAsync(Guid couponId);
    Task<bool> IsCouponCodeExistAsync(string dtoCode);
    Task CreatCouponAsync(Domain.Entities.Coupon coupon);
    Task<Domain.Entities.Coupon?> GetCouponByCodeAsync(string dtoCode);
    Task<int> GetUserCouponUsageCountAsync(Guid couponId, Guid userId);
    Task<Domain.Entities.Coupon?> GetCouponByIdAsync(Guid orderCouponId);
}