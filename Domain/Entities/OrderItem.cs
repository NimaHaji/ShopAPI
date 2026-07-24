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
    public OrderItem(Guid productId,Guid orderId, int quantity,decimal price,string productTitle)
    {
        Id = Guid.NewGuid();
        OrderId=orderId;
        Quantity = quantity;
        ProductId = productId;
        Price = price;
        ProductTitle= productTitle;
    }

    public OrderItem()
    {
        
    }
}