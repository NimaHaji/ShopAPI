namespace Application.Features.CouponUsage.Interfaces;

public interface CouponUsageRepositoryContract
{
    Task CreateCouponUsage(Domain.Entities.CouponUsage couponUsage);
    Task<bool> IsExistCouponUsageByOrderId(Guid orderId);
}