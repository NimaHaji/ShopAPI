namespace Domain.Entites;

public class Cart
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreateAt { get; set; }
    public List<CartItem> CartItems { get; set; } = new();

    public Cart()
    {
        CreateAt = DateTime.UtcNow;
    }
}