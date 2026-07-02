namespace Domain.Entites;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public long TotalPrice { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public DateTime CreateAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    
    public Order()
    {
        Id=Guid.NewGuid();
        CreateAt=DateTime.UtcNow;
        OrderStatus = OrderStatus.Pending;
    }
    
    public void AddItem(OrderItem item)
    {
        Items.Add(item);
        TotalPrice += (long)(item.Price * item.Quantity);
    }
}