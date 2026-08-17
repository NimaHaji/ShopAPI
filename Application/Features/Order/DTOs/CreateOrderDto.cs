namespace Application.Features.Order.DTOs;

public class CreateOrderDto
{
    public List<OrderItemDto> Items { get; set; }
    public Guid? CouponId { get; set; }
    public string? CouponCode { get; set; }
    public long CouponDiscountAmount { get; set; }
}

public class OrderItemDto
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
}