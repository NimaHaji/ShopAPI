using Domain.Enums;
using Shared.Exceptions;

namespace Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    public long TotalPrice { get; private set; }

    public OrderStatus OrderStatus { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public List<OrderItem> OrderItems { get; private set; } = new();
    public Guid? CouponId { get; private set; }
    public Coupon? Coupon { get; private set; }

    public string? CouponCode { get; private set; }
    public long CouponDiscountAmount { get; private set; }
    public List<Payment> Payments { get; private set; }
    
    public Order(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new BusinessException("شناسه کاربر نامعتبر است.");

        Id = Guid.NewGuid();
        UserId = userId;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;

        OrderStatus = OrderStatus.Pending;
    }

    public void AddItem(OrderItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (item.Quantity <= 0)
            throw new InvalidQuantityException(
                "تعداد محصول باید بیشتر از صفر باشد.");

        OrderItems.Add(item);

        TotalPrice += item.FinalUnitPrice * item.Quantity;

        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeOrderStatusTo(OrderStatus status)
    {
        OrderStatus = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (OrderStatus == OrderStatus.Cancelled)
            throw new BusinessException("سفارش قبلاً لغو شده است.");

        OrderStatus = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void ApplyCoupon(
        Guid couponId,
        string couponCode,
        long discountAmount)
    {
        if (couponId == Guid.Empty)
            throw new BusinessException("شناسه کد تخفیف نامعتبر است.");

        if (string.IsNullOrWhiteSpace(couponCode))
            throw new BusinessException("کد تخفیف نامعتبر است.");

        if (discountAmount <= 0)
            throw new BusinessException("مبلغ تخفیف نامعتبر است.");

        if (discountAmount > TotalPrice)
            discountAmount = TotalPrice;

        CouponId = couponId;
        CouponCode = couponCode;
        CouponDiscountAmount = discountAmount;

        TotalPrice -= discountAmount;

        UpdatedAt = DateTime.UtcNow;
    }
}