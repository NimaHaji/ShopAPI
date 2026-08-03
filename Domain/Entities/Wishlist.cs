namespace Domain.Entities;

public class Wishlist
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public User User { get; private set; }
    public Guid UserId { get; private set; }
    public List<WishlistItem> WishlistItems { get; private set; } = new();

    private Wishlist()
    {
        
    }

    public Wishlist(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}