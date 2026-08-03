namespace Domain.Entities;

public class WishlistItem
{
    public Guid Id { get; private set; }

    public Guid WishlistId { get; private set; }
    public Wishlist Wishlist { get; private set; } = null!;

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public DateTime AddedAt { get; private set; }

    private WishlistItem()
    {
    }

    public WishlistItem(Guid productId, Guid wishlistId)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        WishlistId = wishlistId;
        AddedAt = DateTime.UtcNow;
    }
}