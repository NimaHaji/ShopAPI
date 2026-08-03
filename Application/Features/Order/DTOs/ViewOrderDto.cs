namespace Application.Features.Order.DTOs;

public class ViewOrderListDto
{
    public List<ViewOrderDto> OrderList { get; set; }
}
public class ViewOrderDto
{
    public List<ViewOrderItemsDto> Items { get; set; }
    public long? TotalPrice { get; set; }
    public string? OrderStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class ViewOrderItemsDto
{
    public string ProductTitle { get; set; }
    public int ProductQuantity { get; set; }
    public long Price { get; set; } 
}