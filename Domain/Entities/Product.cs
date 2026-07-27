namespace Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public long Price { get; private set; }
    public decimal? DiscountPercentage { get; private set; }
    public DateTime AddedAt { get; private set; }

    public Guid CategoryId { get; private set; }
    public ProductCategory Category { get; private set; }

    public Guid? BrandId { get; private set; }
    public ProductBrand Brand { get; private set; }

    public static Product Create(
        string title,
        string description,
        long price,
        decimal? discountPercentage,
        Guid categoryId,
        Guid? brandId)
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
            AddedAt = DateTime.UtcNow
        };
    }

}