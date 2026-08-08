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
        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        var productIds = orderDto.Items
            .Select(x => x.ProductId)
            .Distinct()
            .ToList();

        var products = await _productRepository
            .GetProductsWithDiscountByIdsAsync(productIds);

        if (products.Count != productIds.Count)
            throw new NotFoundException(
                "یک یا چند محصول یافت نشد.");

        var order = new Domain.Entities.Order(userId);

        var now = DateTime.UtcNow;

        var productMap = products.ToDictionary(x => x.Id);

        foreach (var item in orderDto.Items)
        {
            if (item.Quantity <= 0)
                throw new BusinessException(
                    "تعداد محصول باید بیشتر از صفر باشد.");

            var product = productMap[item.ProductId];

            var activeDiscount = product.DiscountProducts
                .Select(dp => dp.Discount)
                .FirstOrDefault(d =>
                    d is not null &&
                    !d.IsDeleted &&
                    d.IsActive &&
                    d.StartsAt <= now &&
                    d.EndsAt > now);

            long unitPrice = product.Price;
            long discountAmount = 0;
            long finalUnitPrice = unitPrice;

            if (activeDiscount is not null)
            {
                if (activeDiscount.DiscountType == DiscountType.Percentage)
                {
                    discountAmount = (long)(
                        unitPrice *
                        (activeDiscount.Value / 100));

                    if (activeDiscount.MaxDiscountAmount.HasValue)
                    {
                        discountAmount = Math.Min(
                            discountAmount,
                            (long)activeDiscount.MaxDiscountAmount.Value);
                    }
                }
                else if (activeDiscount.DiscountType ==
                         DiscountType.FixedAmount)
                {
                    discountAmount = Math.Min(
                        (long)activeDiscount.Value,
                        unitPrice);
                }

                finalUnitPrice = unitPrice - discountAmount;
            }

            var orderItem = new OrderItem(
                productId: product.Id,
                orderId: order.Id,
                quantity: item.Quantity,
                unitPrice: unitPrice,
                discountAmount: discountAmount,
                finalUnitPrice: finalUnitPrice,
                productTitle: product.Title
            );

            order.AddItem(orderItem);
        }
        if (orderDto.CouponId.HasValue)
        {
            order.ApplyCoupon(
                orderDto.CouponId.Value,
                orderDto.CouponCode!,
                orderDto.CouponDiscountAmount);
        }
        await _orderRepository.CreateOrderAsync(order);

        return order.Id;
    }

    public async Task<ViewOrderListDto> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllOrders();
        
        if (orders is null || !orders.Any())
        {
            return new ViewOrderListDto
            {
                OrderList = []
            };
        }

        var dto = new ViewOrderListDto
        {
            OrderList = orders.Select(order => new ViewOrderDto
                {
                    Id = order.Id,

                    Items = order.OrderItems
                        .Select(item => new ViewOrderItemsDto
                        {
                            ProductId = item.ProductId,
                            ProductTitle = item.ProductTitle,

                            ProductQuantity = item.Quantity,

                            UnitPrice = item.UnitPrice,

                            DiscountAmount = item.DiscountAmount,

                            FinalUnitPrice = item.FinalUnitPrice,

                            TotalPrice = item.FinalUnitPrice * item.Quantity
                        })
                        .ToList(),

                    TotalPrice = order.TotalPrice,
                    
                    TotalDiscountAmount =
                        order.OrderItems.Sum(item =>
                            item.DiscountAmount * item.Quantity)
                        + order.CouponDiscountAmount,

                    OrderStatus = order.OrderStatus.ToString(),

                    CreatedAt = order.CreatedAt,
                    CouponId = order.CouponId,
                    CouponCode = order.CouponCode,
                    CouponDiscountAmount = order.CouponDiscountAmount,
                })
                .ToList()
        };

        return dto;
    }

    public async Task<ViewOrderListDto> GetAllUserOrdersAsync()
    {
        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        var orders = await _orderRepository.GetOrderByUserIdAsync(userId);

        if (orders is null || !orders.Any())
        {
            return new ViewOrderListDto
            {
                OrderList = []
            };
        }

        var dto = new ViewOrderListDto
        {
            OrderList = orders.Select(order => new ViewOrderDto
                {
                    Id = order.Id,

                    Items = order.OrderItems
                        .Select(item => new ViewOrderItemsDto
                        {
                            ProductId = item.ProductId,
                            ProductTitle = item.ProductTitle,

                            ProductQuantity = item.Quantity,

                            UnitPrice = item.UnitPrice,

                            DiscountAmount = item.DiscountAmount,

                            FinalUnitPrice = item.FinalUnitPrice,

                            TotalPrice = item.FinalUnitPrice * item.Quantity
                        })
                        .ToList(),

                    TotalPrice = order.TotalPrice,
                    
                    TotalDiscountAmount =
                        order.OrderItems.Sum(item =>
                            item.DiscountAmount * item.Quantity)
                        + order.CouponDiscountAmount,

                    OrderStatus = order.OrderStatus.ToString(),

                    CreatedAt = order.CreatedAt,
                    CouponId = order.CouponId,
                    CouponCode = order.CouponCode,
                    CouponDiscountAmount = order.CouponDiscountAmount,
                })
                .ToList()
        };

        return dto;
    }

    public async Task<ViewOrderDto> GetOrderByIdAsync(Guid orderId)
    {
        if (orderId == Guid.Empty)
            throw new BusinessException("شناسه سفارش نامعتبر است.");

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        var order = await _orderRepository.GetOrderByIdAsync(orderId, userId);

        if (order is null)
            throw new NotFoundException("سفارش یافت نشد.");

        var dto = new ViewOrderDto
        {
            Id = order.Id,

            Items = order.OrderItems.Select(item => new ViewOrderItemsDto
            {
                ProductId = item.ProductId,
                ProductTitle = item.ProductTitle,

                ProductQuantity = item.Quantity,

                UnitPrice = item.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                FinalUnitPrice = item.FinalUnitPrice,

                TotalPrice = item.FinalUnitPrice * item.Quantity

            }).ToList(),

            TotalPrice = order.TotalPrice,

            TotalDiscountAmount = order.OrderItems.Sum(
                item => item.DiscountAmount * item.Quantity),

            CreatedAt = order.CreatedAt,

            OrderStatus = order.OrderStatus.ToString()
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