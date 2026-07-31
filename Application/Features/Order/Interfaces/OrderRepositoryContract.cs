namespace Application.Features.Order.Interfaces;

public interface OrderRepositoryContract
{
    Task<Domain.Entities.Order?> GetOrderByIdAsync(Guid orderId,Guid userId);
    Task CreateOrderAsync(Domain.Entities.Order order);
    void UpdateOrder(Domain.Entities.Order order);
    Task SaveAsync();
    Task<List<Domain.Entities.Order>?> GetAllOrders();
    Task<List<Domain.Entities.Order>?> GetOrderByUserIdAsync(Guid userId);
}