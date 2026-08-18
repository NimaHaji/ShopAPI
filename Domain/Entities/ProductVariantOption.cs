using Shared.Exceptions;

namespace Domain.Entities;

public class ProductVariantOption
{
    public Guid Id { get; private set; }

    public Guid ProductVariantId { get; private set; }
    public ProductVariant ProductVariant { get; private set; }

    public Guid ProductOptionId { get; private set; }
    public ProductOption ProductOption { get; private set; }

    public Guid ProductOptionValueId { get; private set; }
    public ProductOptionValue ProductOptionValue { get; private set; }

    private ProductVariantOption()
    {
    }

    private ProductVariantOption(
        Guid productVariantId,
        Guid productOptionId,
        Guid productOptionValueId)
    {
        Id = Guid.NewGuid();
        ProductVariantId = productVariantId;
        ProductOptionId = productOptionId;
        ProductOptionValueId = productOptionValueId;
    }

    public static ProductVariantOption Create(
        Guid productVariantId,
        Guid productOptionId,
        Guid productOptionValueId)
    {
        if (productVariantId == Guid.Empty)
            throw new BusinessException(
                "Variant نامعتبر است.");

        if (productOptionId == Guid.Empty)
            throw new BusinessException(
                "Option نامعتبر است.");

        if (productOptionValueId == Guid.Empty)
            throw new BusinessException(
                "Option Value نامعتبر است.");

        return new ProductVariantOption(
            productVariantId,
            productOptionId,
            productOptionValueId);
    }
}