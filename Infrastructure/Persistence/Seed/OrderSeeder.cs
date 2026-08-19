using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Seed.Models;

namespace Infrastructure.Persistence.Seed;

public class OrderSeeder
{
    private readonly ShopDbContext _context;
    private readonly JsonSeedReader _reader;

    public OrderSeeder(
        ShopDbContext context,
        JsonSeedReader reader)
    {
        _context = context;
        _reader = reader;
    }


    public async Task SeedAsync(SeedContext seedContext)
    {
        var items =
            await _reader.ReadListAsync<OrderSeedDto>("orders.json");


        foreach (var orderDto in items)
        {
            if (!seedContext.Users.TryGetValue(
                    orderDto.UserKey,
                    out var userId))
            {
                throw new InvalidOperationException(
                    $"User not found: {orderDto.UserKey}");
            }


            var order = new Order(
                userId,
                orderDto.ReceiverName,
                orderDto.PhoneNumber,
                orderDto.Province,
                orderDto.City,
                orderDto.AddressLine,
                orderDto.PostalCode);


            foreach (var itemDto in orderDto.Items)
            {
                if (!seedContext.Variants.TryGetValue(
                        itemDto.VariantKey,
                        out var variantId))
                {
                    throw new InvalidOperationException(
                        $"Variant not found: {itemDto.VariantKey}");
                }


                var variant =
                    await _context.Variants
                        .FindAsync(variantId)
                    ?? throw new InvalidOperationException(
                        $"Variant entity not found: {itemDto.VariantKey}");


                var product =
                    await _context.Products
                        .FindAsync(variant.ProductId);


                var orderItem = new OrderItem(
                    productId: variant.ProductId,
                    productVariantId: variant.Id,
                    orderId: order.Id,
                    quantity: itemDto.Quantity,
                    unitPrice: itemDto.UnitPrice,
                    discountAmount: itemDto.DiscountAmount,
                    finalUnitPrice: itemDto.UnitPrice - itemDto.DiscountAmount,
                    productTitle: product?.Title ?? "محصول",
                    
                    productImage: variant.Product.Images
                        .Where(pi => pi.IsPrimary)
                        .Select(pi => pi.ImageLink)
                        .FirstOrDefault(),
                    
                    variantImage: variant.Images
                        .Where(pi => pi.IsPrimary)
                        .Select(pi => pi.ImageUrl)
                        .FirstOrDefault(),
                    
                    options: variant.Options
                        .Select(pvo=>(pvo.ProductOption.Name,pvo.ProductOptionValue.Value))
                        .ToList()
                    );


                order.AddItem(orderItem);
            }


            if (!string.IsNullOrWhiteSpace(orderDto.CouponKey))
            {
                if (!seedContext.Coupons.TryGetValue(
                        orderDto.CouponKey,
                        out var couponId))
                {
                    throw new InvalidOperationException(
                        $"Coupon not found: {orderDto.CouponKey}");
                }


                var coupon =
                    await _context.Coupons.FindAsync(couponId)
                    ?? throw new InvalidOperationException(
                        "Coupon entity not found");


                order.ApplyCoupon(
                    couponId,
                    coupon.Code,
                    orderDto.CouponDiscountAmount ?? 0);
            }


            order.ChangeOrderStatusTo(
                ParseOrderStatus(orderDto.Status));


            seedContext.Orders[orderDto.Key] =
                order.Id;


            await _context.Orders.AddAsync(order);


            if (orderDto.Payment is not null)
            {
                var payment = new Payment(
                    order.TotalPrice,
                    orderDto.Payment.Description,
                    ParsePaymentGateway(
                        orderDto.Payment.Gateway),
                    order.Id);


                payment.GenerateOrderNumber();

                ApplyPaymentStatus(
                    payment,
                    orderDto.Payment.Status);


                await _context.Payments.AddAsync(payment);
            }


            if (orderDto.CouponDiscountAmount.HasValue &&
                !string.IsNullOrWhiteSpace(orderDto.CouponKey))
            {
                var couponUsage =
                    new CouponUsage(
                        seedContext.Coupons[orderDto.CouponKey],
                        userId,
                        order.Id,
                        orderDto.CouponDiscountAmount.Value);


                await _context.CouponUsages.AddAsync(couponUsage);
            }
        }
    }


    private static OrderStatus ParseOrderStatus(string value)
    {
        if (!Enum.TryParse<OrderStatus>(
                value,
                true,
                out var status))
        {
            throw new InvalidOperationException(
                $"Invalid order status: {value}");
        }

        return status;
    }


    private static PaymentGateway ParsePaymentGateway(string value)
    {
        if (!Enum.TryParse<PaymentGateway>(
                value,
                true,
                out var gateway))
        {
            throw new InvalidOperationException(
                $"Invalid payment gateway: {value}");
        }

        return gateway;
    }


    private static void ApplyPaymentStatus(
        Payment payment,
        string status)
    {
        switch (status.ToLowerInvariant())
        {
            case "success":
                payment.MarkAsSuccess();
                break;

            case "failed":
                payment.MarkAsFailed();
                break;
        }
    }
}