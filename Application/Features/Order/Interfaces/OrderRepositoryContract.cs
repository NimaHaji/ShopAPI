namespace Application.Features.Order.Interfaces;

public interface OrderRepositoryContract
{
    Task<Domain.Entites.Order?> GetOrderByIdAsync(Guid orderId);
    Task CreateOrderAsync(Domain.Entites.Order order);
    Task SaveAsync();
}