using Shared.Exceptions;

namespace Domain.Entities;

public class ProductOption
{
    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; }

    public string Name { get; private set; }

    public List<ProductOptionValue> Values { get; private set; } = new();

    private ProductOption()
    {
    }

    private ProductOption(
        Guid productId,
        string name)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Name = name.Trim();
    }

    public static ProductOption Create(
        Guid productId,
        string name)
    {
        if (productId == Guid.Empty)
            throw new BusinessException(
                "محصول نامعتبر است.");

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException(
                "نام Option الزامی است.");

        return new ProductOption(
            productId,
            name);
    }

    public void Edit(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException(
                "نام Option الزامی است.");

        Name = name.Trim();
    }
}