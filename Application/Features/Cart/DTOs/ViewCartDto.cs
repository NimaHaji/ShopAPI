namespace Application.Features.Cart.DTOs;

public class ViewCartDto
{
    public Guid? Id { get; set; }

    public Guid UserId { get; set; }

    public List<ViewCartItemDto> Items { get; set; } = new();
}

public class ViewCartItemDto
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Guid ProductVariantId { get; set; }

    public string ProductTitle { get; set; }

    public string VariantSku { get; set; }

    public long UnitPrice { get; set; }

    public long FinalPrice { get; set; }

    public long DiscountAmount { get; set; }

    public decimal? DiscountPercentage { get; set; }

    public int Quantity { get; set; }

    public long TotalPrice => FinalPrice * Quantity;
}