using Shared.Exceptions;

namespace Domain.Entities;

public class DiscountVariant
{
    public Guid DiscountId { get; private set; }

    public Discount Discount { get; private set; } = null!;


    public Guid ProductVariantId { get; private set; }

    public ProductVariant ProductVariant { get; private set; } = null!;


    private DiscountVariant()
    {
    }

    public DiscountVariant(
        Guid discountId,
        Guid productVariantId)
    {
        DiscountId = discountId;
        ProductVariantId = productVariantId;
    }

    public static DiscountVariant Create(
        Guid discountId,
        Guid productVariantId)
    {
        if (discountId == Guid.Empty)
            throw new BusinessException(
                "تخفیف نامعتبر است.");

        if (productVariantId == Guid.Empty)
            throw new BusinessException(
                "Variant نامعتبر است.");

        return new DiscountVariant(
            discountId,
            productVariantId);
    }
}