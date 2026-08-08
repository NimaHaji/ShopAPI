using Domain.Enums;

namespace Application.Features.Coupon.DTOs;

public class EditCouponDto
{
    public Guid Id { get; set; }
    public string? Code { get; set; } = null!;

    public DiscountType? DiscountType { get; set; }

    public decimal? Value { get; set; }

    public decimal? MinimumOrderAmount { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public int? UsageLimit { get; set; }

    public int? UserUsageLimit { get; set; }

    public DateTime? StartsAt { get; set; }

    public DateTime? EndAt { get; set; }
}