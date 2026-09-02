using System.Diagnostics;
using Application.Common.Interfaces;
using Application.Common.Observability;
using Application.Features.Order.DTOs;
using Application.Features.Order.Interfaces;
using Application.Features.Product.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Exceptions;

namespace Application.Features.Order.implementations;

public class OrderService : OrderServicesContract
{
    private readonly IUSerContext _userContext;
    private readonly UnitOfWorkContract _unitOfWorkContract;
    private readonly OrderRepositoryContract _orderRepository;
    private readonly ProductRepositoryContract _productRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(OrderRepositoryContract orderRepository, ProductRepositoryContract productRepository,
        IUSerContext userContext, UnitOfWorkContract unitOfWorkContract)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userContext = userContext;
        _unitOfWorkContract = unitOfWorkContract;
    }

    public async Task<Guid> CreateOrderAsync(
        CreateOrderDto orderDto,
        Domain.Entities.Address userAddress)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(CreateOrderAsync));

        activity?.SetTag(
            "order.items_count",
            orderDto.Items.Count);

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);

        _logger.LogInformation(
            LogEvents.Order.CreateStarted,
            "Order creation started. ItemCount: {ItemCount}",
            orderDto.Items.Count);

        var productVariantIds = orderDto.Items
            .Select(x => x.ProductVariantId)
            .Distinct()
            .ToList();

        var variants = await _productRepository
            .GetVariantsWithDiscountAsync(productVariantIds);

        if (variants.Count != productVariantIds.Count)
        {
            _logger.LogWarning(
                LogEvents.Order.ProductNotFound,
                "Order creation rejected because one or more product variants were not found.");

            throw new NotFoundException(
                "یک یا چند محصول یافت نشد.");
        }

        var order = new Domain.Entities.Order(
            userId: userId,
            receiverName: userAddress.ReceiverName,
            phoneNumber: userAddress.PhoneNumber,
            province: userAddress.Province,
            city: userAddress.City,
            addressLine: userAddress.AddressLine,
            postalCode: userAddress.PostalCode
        );

        var now = DateTime.UtcNow;

        var variantMap = variants.ToDictionary(x => x.Id);

        foreach (var item in orderDto.Items)
        {
            if (item.Quantity <= 0)
            {
                _logger.LogWarning(
                    LogEvents.Order.InvalidQuantity,
                    "Order creation rejected because product quantity is invalid. ProductVariantId: {ProductVariantId}, Quantity: {Quantity}",
                    item.ProductVariantId,
                    item.Quantity);

                throw new BusinessException(
                    "تعداد محصول باید بیشتر از صفر باشد.");
            }

            if (!variantMap.TryGetValue(
                    item.ProductVariantId,
                    out var variant))
            {
                _logger.LogWarning(
                    LogEvents.Order.ProductNotFound,
                    "Order creation rejected because product variant was not found. ProductVariantId: {ProductVariantId}",
                    item.ProductVariantId);

                throw new NotFoundException(
                    "Variant موردنظر یافت نشد.");
            }

            var product = variant.Product;

            var unitPrice = variant.Price;

            long discountAmount = 0;

            var variantDiscount = variant.DiscountVariants
                .Select(dv => dv.Discount)
                .FirstOrDefault(d =>
                    d is not null &&
                    !d.IsDeleted &&
                    d.IsActive &&
                    d.StartsAt <= now &&
                    d.EndsAt > now);

            var productDiscount = product.DiscountProducts
                .Select(dp => dp.Discount)
                .FirstOrDefault(d =>
                    d is not null &&
                    !d.IsDeleted &&
                    d.IsActive &&
                    d.StartsAt <= now &&
                    d.EndsAt > now);

            var activeDiscount =
                variantDiscount ?? productDiscount;

            if (activeDiscount is not null)
            {
                if (activeDiscount.DiscountType ==
                    DiscountType.Percentage)
                {
                    discountAmount = (long)(
                        unitPrice *
                        (activeDiscount.Value / 100m));

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
            }

            var finalUnitPrice =
                unitPrice - discountAmount;

            var orderItem = new OrderItem(
                productId: product.Id,
                productVariantId: variant.Id,
                orderId: order.Id,
                quantity: item.Quantity,
                unitPrice: unitPrice,
                discountAmount: discountAmount,
                finalUnitPrice: finalUnitPrice,
                productTitle: product.Title,
                productImage: product.Images
                    .Where(i => i.IsPrimary)
                    .Select(i => i.ImageLink)
                    .FirstOrDefault(),
                variantImage: variant.Images
                    .Where(i => i.IsPrimary)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault(),
                options: variant.Options
                    .Select(x =>
                        (x.ProductOption.Name,
                            x.ProductOptionValue.Value))
                    .ToList()
            );

            order.AddItem(orderItem);
        }

        activity?.SetTag(
            "order.items_created",
            orderDto.Items.Count);

        if (orderDto.CouponId.HasValue)
        {
            order.ApplyCoupon(
                orderDto.CouponId.Value,
                orderDto.CouponCode!,
                orderDto.CouponDiscountAmount);

            activity?.SetTag(
                "order.has_coupon",
                true);
        }

        await _orderRepository.CreateOrderAsync(order);

        activity?.SetTag(
            "order.id",
            order.Id);

        activity?.SetStatus(
            ActivityStatusCode.Ok);

        _logger.LogInformation(
            LogEvents.Order.CreateCompleted,
            "Order created successfully. OrderId: {OrderId}, ItemCount: {ItemCount}",
            order.Id,
            orderDto.Items.Count);

        return order.Id;
    }


    public async Task<ViewOrderListDto> GetAllOrdersAsync()
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(GetAllOrdersAsync));

        var orders =
            await _orderRepository.GetAllOrders();

        var orderList = orders?
            .Select(order => new ViewOrderDto
            {
                Id = order.Id,

                Items = order.OrderItems
                    .Select(item => new ViewOrderItemDto
                    {
                        ProductId = item.ProductId,
                        ProductVariantId = item.ProductVariantId,
                        ProductTitle = item.ProductTitle,
                        ProductImage = item.ProductImage,
                        VariantImage = item.VariantImage,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        DiscountAmount = item.DiscountAmount,
                        FinalUnitPrice = item.FinalUnitPrice,
                        TotalPrice = item.TotalPrice,

                        Options = item.Options
                            .Select(option =>
                                new ViewOrderItemOptionDto
                                {
                                    OptionName = option.OptionName,
                                    Value = option.Value
                                })
                            .ToList()
                    })
                    .ToList(),

                Subtotal = order.OrderItems.Sum(item =>
                    item.UnitPrice * item.Quantity),

                ProductDiscountAmount = order.OrderItems.Sum(item =>
                    item.DiscountAmount * item.Quantity),

                CouponDiscountAmount =
                    order.CouponDiscountAmount,

                TotalPrice = order.TotalPrice,

                OrderStatus =
                    order.OrderStatus.ToString(),

                CreatedAt = order.CreatedAt,

                CouponId = order.CouponId,
                CouponCode = order.CouponCode,

                ReceiverName = order.ReceiverName,
                PhoneNumber = order.PhoneNumber,
                Province = order.Province,
                City = order.City,
                AddressLine = order.AddressLine,
                PostalCode = order.PostalCode
            })
            .ToList() ?? [];

        activity?.SetTag(
            "orders.count",
            orderList.Count);

        return new ViewOrderListDto
        {
            OrderList = orderList
        };
    }


    public async Task<ViewOrderListDto> GetAllUserOrdersAsync()
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(GetAllUserOrdersAsync));

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);

        var orders =
            await _orderRepository.GetOrderByUserIdAsync(userId);

        var orderList = orders?
            .Select(order => new ViewOrderDto
            {
                Id = order.Id,

                Items = order.OrderItems
                    .Select(item => new ViewOrderItemDto
                    {
                        ProductId = item.ProductId,
                        ProductVariantId = item.ProductVariantId,
                        ProductTitle = item.ProductTitle,
                        ProductImage = item.ProductImage,
                        VariantImage = item.VariantImage,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        DiscountAmount = item.DiscountAmount,
                        FinalUnitPrice = item.FinalUnitPrice,
                        TotalPrice = item.TotalPrice,

                        Options = item.Options
                            .Select(option =>
                                new ViewOrderItemOptionDto
                                {
                                    OptionName = option.OptionName,
                                    Value = option.Value
                                })
                            .ToList()
                    })
                    .ToList(),

                Subtotal = order.OrderItems.Sum(item =>
                    item.UnitPrice * item.Quantity),

                ProductDiscountAmount = order.OrderItems.Sum(item =>
                    item.DiscountAmount * item.Quantity),

                CouponDiscountAmount =
                    order.CouponDiscountAmount,

                TotalPrice = order.TotalPrice,

                OrderStatus =
                    order.OrderStatus.ToString(),

                CreatedAt = order.CreatedAt,

                CouponId = order.CouponId,
                CouponCode = order.CouponCode,

                ReceiverName = order.ReceiverName,
                PhoneNumber = order.PhoneNumber,
                Province = order.Province,
                City = order.City,
                AddressLine = order.AddressLine,
                PostalCode = order.PostalCode
            })
            .ToList() ?? [];

        activity?.SetTag(
            "orders.count",
            orderList.Count);

        return new ViewOrderListDto
        {
            OrderList = orderList
        };
    }


    public async Task<ViewOrderDto> GetOrderByIdAsync(
        Guid orderId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(GetOrderByIdAsync));

        activity?.SetTag("order.id", orderId);

        if (orderId == Guid.Empty)
            throw new BusinessException(
                "شناسه سفارش نامعتبر است.");

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);

        var order =
            await _orderRepository.GetOrderByIdAsync(
                orderId,
                userId);

        if (order is null)
        {
            _logger.LogWarning(
                LogEvents.Order.NotFound,
                "Order not found. OrderId: {OrderId}",
                orderId);

            throw new NotFoundException(
                "سفارش یافت نشد.");
        }

        return new ViewOrderDto
        {
            Id = order.Id,

            Items = order.OrderItems
                .Select(item => new ViewOrderItemDto
                {
                    ProductId = item.ProductId,
                    ProductVariantId = item.ProductVariantId,
                    ProductTitle = item.ProductTitle,
                    ProductImage = item.ProductImage,
                    VariantImage = item.VariantImage,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    DiscountAmount = item.DiscountAmount,
                    FinalUnitPrice = item.FinalUnitPrice,
                    TotalPrice = item.TotalPrice,

                    Options = item.Options
                        .Select(option =>
                            new ViewOrderItemOptionDto
                            {
                                OptionName = option.OptionName,
                                Value = option.Value
                            })
                        .ToList()
                })
                .ToList(),

            Subtotal = order.OrderItems.Sum(item =>
                item.UnitPrice * item.Quantity),

            ProductDiscountAmount = order.OrderItems.Sum(item =>
                item.DiscountAmount * item.Quantity),

            CouponDiscountAmount =
                order.CouponDiscountAmount,

            TotalPrice = order.TotalPrice,

            OrderStatus =
                order.OrderStatus.ToString(),

            CreatedAt = order.CreatedAt,

            CouponId = order.CouponId,
            CouponCode = order.CouponCode,

            ReceiverName = order.ReceiverName,
            PhoneNumber = order.PhoneNumber,
            Province = order.Province,
            City = order.City,
            AddressLine = order.AddressLine,
            PostalCode = order.PostalCode
        };
    }


    public async Task<string> CancelOrderAsync(Guid orderId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(CancelOrderAsync));

        activity?.SetTag("order.id", orderId);

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);

        _logger.LogInformation(
            LogEvents.Order.CancelStarted,
            "Order cancellation started. OrderId: {OrderId}",
            orderId);

        var order =
            await _orderRepository.GetOrderByIdAsync(
                orderId,
                userId);

        if (order is null)
        {
            _logger.LogWarning(
                LogEvents.Order.NotFound,
                "Order cancellation rejected because order was not found. OrderId: {OrderId}",
                orderId);

            throw new NotFoundException(
                "سفارش یافت نشد");
        }

        order.Cancel();

        await _unitOfWorkContract.SaveAsync();

        activity?.SetStatus(
            ActivityStatusCode.Ok);

        _logger.LogInformation(
            LogEvents.Order.CancelCompleted,
            "Order cancelled successfully. OrderId: {OrderId}",
            orderId);

        return "سفارش با موفقیت لغو شد .";
    }


    public async Task<string> ChangOrderStatusByIdAsync(
        Guid orderId,
        OrderStatus status)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(ChangOrderStatusByIdAsync));

        activity?.SetTag("order.id", orderId);
        activity?.SetTag(
            "order.new_status",
            status.ToString());

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);

        _logger.LogInformation(
            LogEvents.Order.StatusChangeStarted,
            "Order status change started. OrderId: {OrderId}, NewStatus: {Status}",
            orderId,
            status);

        var order =
            await _orderRepository.GetOrderByIdAsync(
                orderId,
                userId);

        if (order is null)
        {
            _logger.LogWarning(
                LogEvents.Order.NotFound,
                "Order status change rejected because order was not found. OrderId: {OrderId}",
                orderId);

            throw new NotFoundException(
                "سفارش یافت نشد");
        }

        order.ChangeOrderStatusTo(status);

        await _unitOfWorkContract.SaveAsync();

        activity?.SetStatus(
            ActivityStatusCode.Ok);

        _logger.LogInformation(
            LogEvents.Order.StatusChangeCompleted,
            "Order status changed successfully. OrderId: {OrderId}, Status: {Status}",
            orderId,
            order.OrderStatus);

        return
            $"وضعیت سفارش تغییر پیدا کرد وضعیت فعلی {order.OrderStatus} .";
    }
}