using Application.Features.Order.DTOs;
using Application.Features.Order.Interfaces;
using Application.Features.Product.Interfaces;
using Domain.Entities;

namespace Application.Features.Order.implementations;

public class OrderService:OrderServicesContract
{
    private readonly OrderRepositoryContract _orderRepository;
    private readonly ProductRepositoryContract _productRepository;
    public OrderService(OrderRepositoryContract orderRepository, ProductRepositoryContract productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<Guid> CreateOrderAsync(CreateOrderDto orderDto)
    {
        var order = new Domain.Entities.Order();

        foreach (var item in orderDto.Items)
        {
            var product=await _productRepository.GetProductByIdAsync(item.ProductId);
            var orderItem = new OrderItem(product.Id,item.Quantity,product.Price);
            order.TotalPrice += product.Price * item.Quantity;
            order.AddItem(orderItem);
        }
        await _orderRepository.CreateOrderAsync(order);
        await _orderRepository.SaveAsync();
        
        return order.Id;
    }
}