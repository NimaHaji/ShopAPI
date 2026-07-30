using Domain.Enums;
using Shared.Exceptions;

namespace Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public long TotalPrice { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public DateTime CreateAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();

    public Order(Guid userId)
    {
        UserId = userId;
        Id = Guid.NewGuid();
        CreateAt = DateTime.UtcNow;
        OrderStatus = OrderStatus.Pending;
    }

    public void AddItem(OrderItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (item.Quantity <= 0)
            throw new InvalidQuantityException("تعداد نمیتواند منفی باشد .");

        Items.Add(item);
        TotalPrice += (long)(item.Price * item.Quantity);
    }

    public void MarkAsSucceeded()
    {
        OrderStatus = OrderStatus.Paid;
    }

    public void MarkAsFailed()
    {
        OrderStatus = OrderStatus.Paid;
    }
}