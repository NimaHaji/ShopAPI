using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;

namespace Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public long Price { get; private set; }
    // Todo : Discount System implement
    public decimal? DiscountPercentage { get; private set; }
    public DateTime AddedAt { get; private set; }
    public DateTime UpdatedAt { get;private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid CategoryId { get; private set; }
    public ProductCategory Category { get; private set; }
    public Guid? BrandId { get; private set; }
    public ProductBrand Brand { get; private set; }
    public InventoryItem InventoryItem { get;private set; }
    public List<ProductImage> Images { get; private set; } = new();
    public string Sku { get;private set; }
    [Timestamp] public byte[] RowVersion { get; set; }
    
    
    public static Product Create(
        string title,
        string description,
        long price,
        decimal? discountPercentage,
        Guid categoryId,
        Guid? brandId,
        string sku)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Price = price,
            DiscountPercentage = discountPercentage,
            CategoryId = categoryId,
            BrandId = brandId,
            AddedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false,
            Sku = sku
        };
    }

    public void Edit(string? title, string? description, long? price, decimal? discountPercentage)
    {
        if (title is not null) Title = title;
        if (description is not null) Description = description;
        if (price is not null) Price = price.Value;
        DiscountPercentage = discountPercentage;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}