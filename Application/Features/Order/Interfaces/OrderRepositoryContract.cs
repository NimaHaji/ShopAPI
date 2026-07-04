namespace Application.Features.Order.Interfaces;

public interface OrderRepositoryContract
{
    Task<Domain.Entities.Order?> GetOrderByIdAsync(Guid orderId);
    Task CreateOrderAsync(Domain.Entities.Order order);
    Task SaveAsync();
}