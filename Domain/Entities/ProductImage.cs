using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class ProductImage
{
    public Guid Id { get; private set; }
    public string ImageLink { get; private set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    public bool IsPrimary { get; private set; }
    public int SortOrder { get; private set; }
    public ProductImage(
        Guid productId,
        string imageLink,
        bool isPrimary,
        int sortOrder)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ImageLink = imageLink;
        IsPrimary = isPrimary;
        SortOrder = sortOrder;
    }
}