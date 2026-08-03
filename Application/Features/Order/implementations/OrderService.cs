using System.Xml.Schema;
using Application.Common.Interfaces;
using Application.Features.Order.DTOs;
using Application.Features.Order.Interfaces;
using Application.Features.Product.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Shared.Exceptions;

namespace Application.Features.Order.implementations;

public class OrderService : OrderServicesContract
{
    private readonly IUSerContext _userContext;
    private readonly UnitOfWorkContract _unitOfWorkContract;
    private readonly OrderRepositoryContract _orderRepository;
    private readonly ProductRepositoryContract _productRepository;

    public OrderService(OrderRepositoryContract orderRepository, ProductRepositoryContract productRepository,
        IUSerContext userContext, UnitOfWorkContract unitOfWorkContract)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userContext = userContext;
        _unitOfWorkContract = unitOfWorkContract;
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

    public async Task<ViewOrderListDto> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllOrders();

        if (orders is null)
            return new ViewOrderListDto
            {
                OrderList = []
            };

        var dto = new ViewOrderListDto
        {
            OrderList = orders.Select(order => new ViewOrderDto
            {
                Items = order.OrderItems.Select(item => new ViewOrderItemsDto
                {
                    Price = item.Price,
                    ProductQuantity = item.Quantity,
                    ProductTitle = item.ProductTitle
                }).ToList(),
                CreatedAt = order.CreatedAt,
                OrderStatus = order.OrderStatus.ToString(),
                TotalPrice = order.TotalPrice
            }).ToList()
        };

        return dto;
    }

    public async Task<ViewOrderListDto> GetAllUserOrdersAsync()
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");

        var orders = await _orderRepository.GetOrderByUserIdAsync(userId);

        if (orders is null)
            return new ViewOrderListDto
            {
                OrderList = []
            };

        var dto = new ViewOrderListDto
        {
            OrderList = orders.Select(order => new ViewOrderDto
            {
                Items = order.OrderItems.Select(item => new ViewOrderItemsDto
                {
                    Price = item.Price,
                    ProductQuantity = item.Quantity,
                    ProductTitle = item.ProductTitle
                }).ToList(),
                CreatedAt = order.CreatedAt,
                OrderStatus = order.OrderStatus.ToString(),
                TotalPrice = order.TotalPrice
            }).ToList()
        };

        return dto;
    }

    public async Task<ViewOrderDto> GetOrderByIdAsync(Guid orderId)
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");

        var order = await _orderRepository.GetOrderByIdAsync(orderId, userId);

        if (order is null)
            throw new NotFoundException("سفارش یافت نشد");

        var dto = new ViewOrderDto
        {
            Items = order.OrderItems.Select(item => new ViewOrderItemsDto
            {
                Price = item.Price,
                ProductQuantity = item.Quantity,
                ProductTitle = item.ProductTitle
            }).ToList(),
            CreatedAt = order.CreatedAt,
            OrderStatus = order.OrderStatus.ToString(),
            TotalPrice = order.TotalPrice
        };

        return dto;
    }

    public async Task<string> CancelOrderAsync(Guid orderId)
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");

        var order = await _orderRepository.GetOrderByIdAsync(orderId, userId);

        if (order is null)
            throw new NotFoundException("سفارش یافت نشد");

        order.Cancel();
        await _unitOfWorkContract.SaveAsync();
        return "سفارش با موفقیت لغو شد .";
    }

    public async Task<string> ChangOrderStatusByIdAsync(Guid orderId, OrderStatus status)
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");

        var order = await _orderRepository.GetOrderByIdAsync(orderId, userId);

        if (order is null)
            throw new NotFoundException("سفارش یافت نشد");

        order.ChangeOrderStatusTo(status);
        await _unitOfWorkContract.SaveAsync();
        return $"وضعیت سفارش تغییر پیدا کرد وضعیت فعلی {order.OrderStatus.ToString()} .";
    }
}