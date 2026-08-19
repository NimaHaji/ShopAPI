using Domain.Entities;
using Shared.Exceptions;

public class OrderItemOption
{
    public Guid Id { get; private set; }

    public Guid OrderItemId { get; private set; }
    public OrderItem OrderItem { get; private set; } = null!;

    public string OptionName { get; private set; } = null!;
    public string Value { get; private set; } = null!;

    private OrderItemOption()
    {
    }

    public OrderItemOption(
        Guid orderItemId,
        string optionName,
        string value)
    {
        if (orderItemId == Guid.Empty)
            throw new BusinessException("شناسه OrderItem نامعتبر است.");

        if (string.IsNullOrWhiteSpace(optionName))
            throw new BusinessException("نام Option الزامی است.");

        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessException("مقدار Option الزامی است.");

        Id = Guid.NewGuid();
        OrderItemId = orderItemId;
        OptionName = optionName.Trim();
        Value = value.Trim();
    }
}