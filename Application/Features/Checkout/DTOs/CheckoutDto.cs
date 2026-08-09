namespace Application.Features.Checkout.DTOs;

public class CheckoutDto
{
    public Guid AddressId { get; set; }
    public string? CouponCode { get; set; }
}