using Shared.Exceptions;

namespace Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public string ProductTitle { get; private set; } = null!;

    public long UnitPrice { get; private set; }
    public long DiscountAmount { get; private set; }
    public long FinalUnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;

    public long TotalPrice => FinalUnitPrice * Quantity;

    public OrderItem(
        Guid productId,
        Guid productVariantId,
        Guid orderId,
        int quantity,
        long unitPrice,
        long discountAmount,
        long finalUnitPrice,
        string productTitle)
    {
        if (productId == Guid.Empty)
            throw new BusinessException("شناسه محصول نامعتبر است.");

        if (productVariantId == Guid.Empty)
            throw new BusinessException("شناسه محصول نامعتبر است.");

        if (orderId == Guid.Empty)
            throw new BusinessException("شناسه سفارش نامعتبر است.");

        if (quantity <= 0)
            throw new BusinessException("تعداد محصول باید بیشتر از صفر باشد.");

        if (unitPrice < 0)
            throw new BusinessException("قیمت محصول نمی‌تواند منفی باشد.");

        if (discountAmount < 0 || discountAmount > unitPrice)
            throw new BusinessException("مقدار تخفیف نامعتبر است.");

        if (finalUnitPrice != unitPrice - discountAmount)
            throw new BusinessException("قیمت نهایی نامعتبر است.");

        Id = Guid.NewGuid();
        ProductId = productId;
        ProductVariantId = productVariantId;
        OrderId = orderId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountAmount = discountAmount;
        FinalUnitPrice = finalUnitPrice;
        ProductTitle = productTitle;
    }

    private OrderItem()
    {
    }
}