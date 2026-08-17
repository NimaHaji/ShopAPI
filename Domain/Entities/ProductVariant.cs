using Shared.Exceptions;

namespace Domain.Entities;

public class ProductVariant
{
    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; }

    public string Sku { get; private set; }

    public long Price { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public DateTime AddedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public InventoryItem InventoryItem { get; private set; }

    public List<ProductVariantOption> Options { get; private set; } = new();
    public List<ProductVariantImage> Images { get; private set; } = new();
    public List<DiscountVariant> DiscountVariants { get; private set; } = new();

    private ProductVariant()
    {
    }

    public static ProductVariant Create(
        Guid productId,
        string sku,
        long price)
    {
        if (productId == Guid.Empty)
            throw new BusinessException(
                "محصول نامعتبر است.");

        if (string.IsNullOrWhiteSpace(sku))
            throw new BusinessException(
                "SKU الزامی است.");

        if (price < 0)
            throw new BusinessException(
                "قیمت نمی‌تواند منفی باشد.");

        return new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Sku = sku.Trim(),
            Price = price,
            AddedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public void Edit(
        string? sku,
        long? price)
    {
        if (sku is not null)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new BusinessException(
                    "SKU نمی‌تواند خالی باشد.");

            Sku = sku.Trim();
        }

        if (price is not null)
        {
            if (price < 0)
                throw new BusinessException(
                    "قیمت نمی‌تواند منفی باشد.");

            Price = price.Value;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        DeletedAt = UpdatedAt;
    }

    public void Restore()
    {
        if (!IsDeleted)
            throw new BusinessException(
                "این Variant حذف نشده است.");

        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetInventoryItem(InventoryItem inventoryItem)
    {
        InventoryItem = inventoryItem;
    }
}