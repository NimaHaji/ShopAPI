using Shared.Exceptions;

namespace Domain.Entities;

public class Cart
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public List<CartItem> CartItems { get; private set; } = new();

    public Cart(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new BusinessException("شناسه کاربر معتبر نیست.");

        Id = Guid.NewGuid();    
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveCartItem(CartItem cartItem)
    {
        ArgumentNullException.ThrowIfNull(cartItem);

        if (!CartItems.Remove(cartItem))
            throw new NotFoundException("محصول در سبد خرید یافت نشد.");

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException(
                "تعداد باید بیشتر از صفر باشد.");

        var item = CartItems
            .FirstOrDefault(x => x.ProductId == productId);

        if (item is null)
            throw new NotFoundException(
                "محصول در سبد خرید یافت نشد.");

        item.SetQuantity(quantity);

        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearCart()
    {
        CartItems.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}