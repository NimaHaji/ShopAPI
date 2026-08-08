namespace Application.Features.Coupon.DTOs;

public class ViewCouponDto
{
    public List<ViewCouponitemsDto> CouponItems { get; set; }
}

public class ViewCouponitemsDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string DiscountType { get; set; }
    public decimal Value { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int? UserUsageLimit { get; set; }
    public int UsedCount { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}