using Application.Features.Order.DTOs;
using Domain.Enums;

namespace Application.Features.Order.Interfaces;

public interface OrderServicesContract
{
    Task<Guid> CreateOrderAsync(CreateOrderDto orderDto,Domain.Entities.Address userAddress);
    Task<ViewOrderListDto> GetAllOrdersAsync();
    Task<ViewOrderListDto> GetAllUserOrdersAsync();
    Task<ViewOrderDto> GetOrderByIdAsync(Guid orderId);
    Task<string> CancelOrderAsync(Guid orderId);
    Task<string> ChangOrderStatusByIdAsync(Guid orderId, OrderStatus status);
}