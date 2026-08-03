namespace Application.Features.Cart.DTOs;

public class ViewCartDto
{
    public Guid? Id { get; set; }
    public Guid UserId { get; set; }
    public List<ViewCartItemsDto> Items { get; set; }
}

public class ViewCartItemsDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; }
    public long UnitPrice { get; set; }
    public int Quantity { get; set; }
    public long TotalPrice => UnitPrice * Quantity;
}