using Application.Common.Interfaces;
using Application.Features.Order.DTOs;
using Application.Features.Order.Interfaces;
using Application.Features.Product.Interfaces;
using Domain.Entities;
using Shared.Exceptions;

namespace Application.Features.Order.implementations;

public class OrderService : OrderServicesContract
{
    private readonly IUSerContext  _userContext;
    private readonly OrderRepositoryContract _orderRepository;
    private readonly ProductRepositoryContract _productRepository;

    public OrderService(OrderRepositoryContract orderRepository, ProductRepositoryContract productRepository, IUSerContext userContext)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userContext = userContext;
    }

    public async Task<Guid> CreateOrderAsync(CreateOrderDto orderDto)
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");
        var productIds = orderDto.Items.Select(x => x.ProductId).Distinct().ToList();

        var products = await _productRepository.GetProductsByIdsAsync(productIds);
        if (products.Count != productIds.Count)
            throw new NotFoundException("یک یا چند محصول یافت نشد .");

        var order = new Domain.Entities.Order(userId);

        var productMap = products.ToDictionary(x => x.Id);
        foreach (var item in orderDto.Items)
        {
            var product = productMap[item.ProductId];
            var orderItem = new OrderItem(
                productId: product.Id,
                orderId: order.Id,
                quantity: item.Quantity,
                price: product.Price,
                productTitle: product.Title
            );
            order.AddItem(orderItem);
        }

        await _orderRepository.CreateOrderAsync(order);
        return order.Id;
    }
}