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

    public List<OrderItem> OrderItems { get; private set; } = [];
    public Guid? CouponId { get; private set; }
    public Coupon? Coupon { get; private set; }
    
    public string ReceiverName { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Province { get; private set; }
    public string City { get; private set; }
    public string AddressLine { get; private set; }
    public string PostalCode { get; private set; }
    
    public string? CouponCode { get; private set; }
    public long CouponDiscountAmount { get; private set; }
    public List<Payment> Payments { get; private set; } = [];

    private Order()
    {
        
    }
    public Order(Guid userId,
        string receiverName,
        string phoneNumber,
        string province,
        string city,
        string addressLine,
        string postalCode)
    {
        if (userId == Guid.Empty)
            throw new BusinessException("شناسه کاربر نامعتبر است.");

        if (string.IsNullOrWhiteSpace(receiverName))
            throw new BusinessException("نام دریافت کننده نمی تواند خالی باشد .");    
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new BusinessException("موبایل دریافت کننده نمی تواند خالی باشد .");
        
        if (string.IsNullOrWhiteSpace(province))
            throw new BusinessException("نام استان نمی تواند خالی باشد .");
        
        if (string.IsNullOrWhiteSpace(city))
            throw new BusinessException("نام شهر نمی تواند خالی باشد .");
        
        if (string.IsNullOrWhiteSpace(addressLine))
            throw new BusinessException("آدرس نمی تواند خالی باشد .");
        
        if (string.IsNullOrWhiteSpace(addressLine))
            throw new BusinessException("کد پستی نمی تواند خالی باشد .");
        
        Id = Guid.NewGuid();
        UserId = userId;
        
        ReceiverName=receiverName;
        PhoneNumber=phoneNumber;
        Province=province;
        City=city;
        AddressLine=addressLine;
        PostalCode=postalCode;
        
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