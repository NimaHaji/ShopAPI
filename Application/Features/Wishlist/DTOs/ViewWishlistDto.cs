namespace Application.Features.Wishlist.DTOs;

public class ViewWishlistDto
{
    public Guid? Id { get; set; }

    public List<ViewWishlistItemDto> WishlistItems { get; set; } = new();
}

public class ViewWishlistItemDto
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime AddedAt { get; set; }
}