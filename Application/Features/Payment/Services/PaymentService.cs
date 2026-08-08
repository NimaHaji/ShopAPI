using Application.Common.Interfaces;
using Application.Features.Coupon.Interfaces;
using Application.Features.CouponUsage.Interfaces;
using Application.Features.Order.Interfaces;
using Application.Features.Payment.DTOs;
using Application.Features.Payment.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Shared.Exceptions;

namespace Application.Features.Payment.Services;

public class PaymentService : PaymentServiceContract
{
    private readonly PaymentRepositoryContract _paymentRepository;
    private readonly PaymentGatewayResolverContract _gatewayResolver;
    private readonly UnitOfWorkContract _unitOfWorkContract;
    private readonly OrderRepositoryContract _orderRepository;
    private readonly IUSerContext _userContext;
    private readonly CouponUsageRepositoryContract _couponUsageRepositoryContract;
    private readonly CouponRepositoryContract _couponRepositoryContract;

    public PaymentService(PaymentRepositoryContract paymentRepository, PaymentGatewayResolverContract gatewayResolver,
        OrderRepositoryContract orderRepository, IUSerContext userContext, UnitOfWorkContract unitOfWorkContract,
        CouponUsageRepositoryContract couponUsageRepositoryContract, CouponRepositoryContract couponRepositoryContract)
    {
        _paymentRepository = paymentRepository;
        _gatewayResolver = gatewayResolver;
        _orderRepository = orderRepository;
        _userContext = userContext;
        _unitOfWorkContract = unitOfWorkContract;
        _couponUsageRepositoryContract = couponUsageRepositoryContract;
        _couponRepositoryContract = couponRepositoryContract;
    }

    public async Task<string?> CreatePaymentAsync(CreatePaymentDto dto)
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");

        var order = await _orderRepository.GetOrderByIdAsync(dto.OrderId, userId);
        if (order is null)
            throw new NotFoundException("سفارشی یافت نشد");

        var payment = new Domain.Entities.Payment(order.TotalPrice, dto.Description, dto.Gateway, order.Id);

        var provider = _gatewayResolver.Resolve(dto.Gateway);

        var requestResult = await provider.RequestPaymentAsync(payment, dto);

        if (!requestResult.IsSuccess)
        {
            payment.MarkAsFailed();
            await _unitOfWorkContract.SaveAsync();

            throw new InvalidOperationException(requestResult.ErrorMessage);
        }

        payment.SetAuthority(requestResult.GatewayToken);

        await _paymentRepository.CreatePaymentAsync(payment);
        await _unitOfWorkContract.SaveAsync();

        return requestResult.PaymentUrl;
    }

    public async Task<VerifyPaymentResult> HandleCallBackAsync(PaymentGateway gateway, SandBoxCallBackDto dto)
    {
        var provider = _gatewayResolver.Resolve(gateway);
        var result = await provider.HandleCallBackAsync(dto);

        if (!result.IsSuccess)
            return result;

        var payment = await _paymentRepository.GetPaymentByAuthorityAsync(dto.Authority);

        if (payment is null)
            return VerifyPaymentResult.Failed("تراکنش یافت نشد .");

        var order = await _orderRepository.GetOrderByIdAsync(payment.OrderId);

        if (order is null)
            return VerifyPaymentResult.Failed("سفارش مرتبط با پرداخت یافت نشد .");
        try
        {
            await _unitOfWorkContract.BeginTransactionAsync();
            if (order.OrderStatus == OrderStatus.Paid)
            {
                await _unitOfWorkContract.CommitTransactionAsync();
                return result;
            }

            order.ChangeOrderStatusTo(OrderStatus.Paid);

            if (order.CouponId.HasValue)
            {
                var coupon = await _couponRepositoryContract.GetCouponByIdAsync(order.CouponId.Value);
                if (coupon is null)
                    throw new NotFoundException("کد تخفیف یافت نشد .");

                var isExistCouponUsage = await _couponUsageRepositoryContract.IsExistCouponUsageByOrderId(order.Id);

                if (!isExistCouponUsage)
                {
                    var couponUsage = new Domain.Entities.CouponUsage(
                        couponId: order.CouponId.Value,
                        userId: order.UserId,
                        orderId: order.Id,
                        discountAmount: order.CouponDiscountAmount
                    );

                    coupon.IncreaseUsage();
                    await _couponUsageRepositoryContract.CreateCouponUsage(couponUsage);
                }
            }

            await _unitOfWorkContract.SaveAsync();
            await _unitOfWorkContract.CommitTransactionAsync();
            return result;
        }
        catch
        {
            await _unitOfWorkContract.RollbackTransactionAsync();
            throw;
        }
    }
}