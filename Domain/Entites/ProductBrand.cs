namespace Domain.Entites;

public class ProductBrand   
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products;
    private ProductBrand(){}
    public static ProductBrand Create(string title)
    {
        return new ProductBrand
        {
            Id = Guid.NewGuid(),
            Title = title
        };
    }

}