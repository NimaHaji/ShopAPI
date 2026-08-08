namespace Application.Features.Order.DTOs;

public class ViewOrderListDto
{
    public List<ViewOrderDto> OrderList { get; set; } = [];
}
public class ViewOrderDto
{
    public Guid Id { get; set; }
    public List<ViewOrderItemsDto> Items { get; set; } = [];
    public long TotalPrice { get; set; }
    public long TotalDiscountAmount { get; set; }
    public string? OrderStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public Guid? CouponId { get; set; }
    public string? CouponCode { get; set; }
    public long CouponDiscountAmount { get; set; }
}

public class ViewOrderItemsDto
{
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = null!;
    
    public int ProductQuantity { get; set; }
    public long UnitPrice { get; set; }
    public long DiscountAmount { get; set; }
    public long FinalUnitPrice { get; set; }
    public long TotalPrice { get; set; }
}