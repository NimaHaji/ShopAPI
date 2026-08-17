using Shared.Exceptions;

namespace Domain.Entities;

public class ProductVariantImage
{
    public Guid Id { get; private set; }

    public Guid ProductVariantId { get; private set; }
    public ProductVariant ProductVariant { get; private set; }

    public string ImageUrl { get; private set; }

    public bool IsPrimary { get; private set; }

    public int SortOrder { get; private set; }

    private ProductVariantImage()
    {
    }

    private ProductVariantImage(
        Guid productVariantId,
        string imageUrl,
        bool isPrimary,
        int sortOrder)
    {
        Id = Guid.NewGuid();
        ProductVariantId = productVariantId;
        ImageUrl = imageUrl.Trim();
        IsPrimary = isPrimary;
        SortOrder = sortOrder;
    }

    public static ProductVariantImage Create(
        Guid productVariantId,
        string imageUrl,
        bool isPrimary,
        int sortOrder)
    {
        if (productVariantId == Guid.Empty)
            throw new BusinessException(
                "Variant نامعتبر است.");

        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new BusinessException(
                "آدرس تصویر الزامی است.");

        if (sortOrder < 0)
            throw new BusinessException(
                "ترتیب تصویر نامعتبر است.");

        return new ProductVariantImage(
            productVariantId,
            imageUrl,
            isPrimary,
            sortOrder);
    }
}