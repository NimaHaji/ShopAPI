using System.Security.AccessControl;

namespace Domain.Entites;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
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
}