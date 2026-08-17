using System.ComponentModel.DataAnnotations;
using Shared.Exceptions;

namespace Domain.Entities;

public class InventoryItem
{
    public Guid InventoryId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public ProductVariant ProductVariant { get; private set; }
    public int StockQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public DateTime LastUpdated { get; private set; }
    [Timestamp] public byte[] RowVersion { get; private set; }

    public List<InventoryTransaction> Transactions { get; private set; } = new();
    
    public int AvailableQuantity => StockQuantity - ReservedQuantity;

    public InventoryItem(Guid productVariantId, int stockQuantity, int reservedQuantity)
    {
        InventoryId = Guid.NewGuid();
        LastUpdated = DateTime.UtcNow;
        ProductVariantId = productVariantId;
        StockQuantity = stockQuantity;
        ReservedQuantity = reservedQuantity;
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessException("تعداد باید بیشتر از صفر باشد");

        if (StockQuantity < quantity)
            throw new InsufficientStockException("موجودی محصول کافی نیست .");

        ReservedQuantity += quantity;
        LastUpdated = DateTime.UtcNow;
    }

    public void CommitReserve(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessException("تعداد باید بیشتر از صفر باشد");

        if (StockQuantity < quantity)
            throw new InsufficientStockException("موجودی محصول کافی نیست .");

        StockQuantity -= quantity;
        ReservedQuantity -= quantity;
        LastUpdated = DateTime.UtcNow;
    }

    public void CancelReserve(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessException("تعداد باید بیشتر از صفر باشد");

        if (StockQuantity < quantity)
            throw new InsufficientStockException("موجودی محصول کافی نیست .");

        ReservedQuantity -= quantity;
        LastUpdated = DateTime.UtcNow;
    }

    public void AddStockQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessException("تعداد باید بیشتر از صفر باشد");

        StockQuantity += quantity;
        LastUpdated = DateTime.UtcNow;
    }
}