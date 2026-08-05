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
}