using Domain.Enums;

namespace Domain.Entities;

public class Coupon
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = null!;
    public DiscountType DiscountType { get; private set; }
    public decimal Value { get; private set; }
    public decimal? MinimumOrderAmount { get; private set; }
    public decimal? MaxDiscountAmount { get; private set; }
    public int? UsageLimit { get; private set; }
    public int? UserUsageLimit { get; private set; }
    public int UsedCount { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public bool IsDeleted { get; private set; }
}