namespace Application.Features.Coupon.DTOs;

public class ValidateCouponResponseDto
{
    public Guid CouponId { get; set; }
    public string Code { get; set; } = null!;
    public decimal CartTotalPrice { get; set; }
    public long DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
}