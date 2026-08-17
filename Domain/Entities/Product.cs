using System.ComponentModel.DataAnnotations;
using Shared.Exceptions;

namespace Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime AddedAt { get; private set; }
    public DateTime UpdatedAt { get;private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid CategoryId { get; private set; }
    public ProductCategory Category { get; private set; }
    public Guid? BrandId { get; private set; }
    public ProductBrand Brand { get; private set; }
    
    public List<ProductImage> Images { get; private set; } = new();
    public List<ProductOption> Options { get; private set; } = new();
    public List<ProductVariant> Variants { get; private set; }= new();
    public List<Review> Reviews { get; private set; } = new();
    public List<DiscountProduct> DiscountProducts { get;private set; } = new();
    
    [Timestamp] public byte[] RowVersion { get; set; }
    public static Product Create(
        string title,
        string description,
        Guid categoryId,
        Guid? brandId)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            CategoryId = categoryId,
            BrandId = brandId,
            AddedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false,
        };
    }

    public void Edit(string? title, string? description)
    {
        if (title is not null) Title = title;
        if (description is not null) Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Restore()
    {
        if (!IsDeleted)
            throw new BusinessException("این محصول حذف نشده است .");
        
        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}