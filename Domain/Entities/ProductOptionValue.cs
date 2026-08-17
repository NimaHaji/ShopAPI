using Shared.Exceptions;

namespace Domain.Entities;

public class ProductOptionValue
{
    public Guid Id { get; private set; }

    public Guid ProductOptionId { get; private set; }
    public ProductOption ProductOption { get; private set; }

    public string Value { get; private set; }

    private ProductOptionValue()
    {
    }

    private ProductOptionValue(
        Guid productOptionId,
        string value)
    {
        Id = Guid.NewGuid();
        ProductOptionId = productOptionId;
        Value = value.Trim();
    }

    public static ProductOptionValue Create(
        Guid productOptionId,
        string value)
    {
        if (productOptionId == Guid.Empty)
            throw new BusinessException(
                "Option نامعتبر است.");

        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessException(
                "مقدار Option الزامی است.");

        return new ProductOptionValue(
            productOptionId,
            value);
    }

    public void Edit(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessException(
                "مقدار Option الزامی است.");

        Value = value.Trim();
    }
}