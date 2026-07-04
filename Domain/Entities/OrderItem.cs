namespace Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
    public OrderItem(Guid productId, int quantity,decimal price)
    {
        Id = Guid.NewGuid();
        Quantity = quantity;
        ProductId = productId;
        Price = price;
    }

    public OrderItem()
    {
        
    }
}