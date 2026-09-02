namespace Domain.Entities;

public class DiscountProduct
{
    public Guid DiscountId { get; private set; }
    public Discount Discount { get; private set; } = null!;

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public DiscountProduct(Guid discountId, Guid productId)
    {
        DiscountId = discountId;
        ProductId = productId;
    }
}