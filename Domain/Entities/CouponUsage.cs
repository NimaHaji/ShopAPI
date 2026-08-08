using Shared.Exceptions;

namespace Domain.Entities;

public class CouponUsage
{
    public Guid Id { get; private set; }

    public Coupon Coupon { get; private set; } = null!;
    public Guid CouponId { get; private set; }

    public User User { get; private set; } = null!;
    public Guid UserId { get; private set; }

    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;

    public decimal DiscountAmount { get; private set; }
    public DateTime UsedAt { get;private set; }
    
    private CouponUsage()
    {
    }

    public CouponUsage(
        Guid couponId,
        Guid userId,
        Guid orderId,
        decimal discountAmount)
    {
        if (couponId == Guid.Empty)
            throw new BusinessException("شناسه کد تخفیف نامعتبر است.");

        if (userId == Guid.Empty)
            throw new BusinessException("شناسه کاربر نامعتبر است.");

        if (orderId == Guid.Empty)
            throw new BusinessException("شناسه سفارش نامعتبر است.");

        if (discountAmount <= 0)
            throw new BusinessException("مبلغ تخفیف نامعتبر است.");

        Id = Guid.NewGuid();
        CouponId = couponId;
        UserId = userId;
        OrderId = orderId;
        DiscountAmount = discountAmount;
        UsedAt = DateTime.UtcNow;
    }
}