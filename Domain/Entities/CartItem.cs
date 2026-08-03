using Shared.Exceptions;

namespace Domain.Entities;

public class CartItem
{
    public Guid Id { get; private set; }
    public Guid CartId { get; private set; }
    public Cart Cart { get; private set; }

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; }

    public int Quantity { get; private set; }

    private CartItem()
    {
    }

    public CartItem(Guid cartId, Guid productId, int quantity)
    {
        if (cartId == Guid.Empty)
            throw new BusinessException("شناسه سبد خرید معتبر نیست.");

        if (productId == Guid.Empty)
            throw new BusinessException("شناسه محصول معتبر نیست.");

        if (quantity <= 0)
            throw new InvalidQuantityException(
                "تعداد باید بیشتر از صفر باشد.");

        Id = Guid.NewGuid();
        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
    }
    
    public void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException("تعداد باید بیشتر از صفر باشد.");

        Quantity += quantity;
    }

    public void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException("تعداد باید بیشتر از صفر باشد.");

        Quantity = quantity;
    }
}