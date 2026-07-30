namespace Domain.Entities;

public class ProductCategory
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime DeletedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products;

    private ProductCategory()
    {
    }

    public static ProductCategory Create(string title)
    {
        return new ProductCategory
        {
            Id = Guid.NewGuid(),
            Title = title,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Edit(string title)
    {
        Title = title;
    }
    
    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}