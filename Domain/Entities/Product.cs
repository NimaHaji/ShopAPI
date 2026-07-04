namespace Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public long Price { get; private set; }
    public decimal? DiscountPercentage { get; private set; }
    public int Stock { get; private set; }
    
    public Guid CategoryId { get; private set; }
    public ProductCategory Category { get; private set; }

    public Guid? BrandId { get; private set; }
    public ProductBrand Brand { get; private set; }

    public decimal Rating { get; private set; }

    public static Product Create(
        string title,
        string description,
        long price,
        decimal? discountPercentage,
        int stock,
        Guid categoryId,
        Guid? brandId,
        decimal rating)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Price = price,
            DiscountPercentage = discountPercentage,
            Stock = stock,
            CategoryId = categoryId,
            BrandId = brandId,
            Rating = rating
        };
    }
}