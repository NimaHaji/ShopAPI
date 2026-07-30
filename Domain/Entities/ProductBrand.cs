namespace Domain.Entities;

public class ProductBrand
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products;
    public bool IsDeleted { get; private set; }
    public DateTime DeletedAt { get; private set; }
    public DateTime CreatedAt { get; set; }

    private ProductBrand()
    {
    }

    public static ProductBrand Create(string title)
    {
        return new ProductBrand
        {
            Id = Guid.NewGuid(),
            Title = title,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Edit(string title)
    {
        Title=title;
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}