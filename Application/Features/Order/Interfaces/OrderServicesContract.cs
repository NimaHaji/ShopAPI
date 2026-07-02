using Application.Features.Order.DTOs;

namespace Application.Features.Order.Interfaces;

public interface OrderServicesContract
{
    Task<Guid> CreateOrderAsync(CreateOrderDto orderDto);
}