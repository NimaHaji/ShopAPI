using Application.Common.Interfaces;
using Application.Features.Coupon.Interfaces;
using Application.Features.CouponUsage.Interfaces;
using Application.Features.IdempotencyKey.DTOs;
using Application.Features.IdempotencyKey.Interfaces;
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
    private readonly IdempotencyServiceContract _idempotencyServiceContract;

    public PaymentService(PaymentRepositoryContract paymentRepository, PaymentGatewayResolverContract gatewayResolver,
        OrderRepositoryContract orderRepository, IUSerContext userContext, UnitOfWorkContract unitOfWorkContract,
        CouponUsageRepositoryContract couponUsageRepositoryContract, CouponRepositoryContract couponRepositoryContract,
        IdempotencyServiceContract idempotencyServiceContract)
    {
        _paymentRepository = paymentRepository;
        _gatewayResolver = gatewayResolver;
        _orderRepository = orderRepository;
        _userContext = userContext;
        _unitOfWorkContract = unitOfWorkContract;
        _couponUsageRepositoryContract = couponUsageRepositoryContract;
        _couponRepositoryContract = couponRepositoryContract;
        _idempotencyServiceContract = idempotencyServiceContract;
    }

    public async Task<string?> CreatePaymentAsync(CreatePaymentDto dto, string idempotencyKey)
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");

        var existing = await _idempotencyServiceContract.CheckAsync(
            userId,
            idempotencyKey,
            IdempotencyOperation.Payment
        );

        if (existing.Status == IdempotencyStatusDto.Processing)
        {
            throw new ConflictException("درخواست دیگری با همین Idempotency-Key در حال پردازش است.");
        }

        if (existing.Status == IdempotencyStatusDto.Completed)
        {
            if (existing.ResourceId.HasValue)
            {
                var previousPayment = await _paymentRepository.GetPaymentByIdAsync(existing.ResourceId.Value);

                if (previousPayment is null)
                    throw new InvalidOperationException("پرداخت مربوط به درخواست قبلی یافت نشد .");

                return previousPayment.PaymentUrl;
            }
        }

        await _unitOfWorkContract.SaveAsync();
        
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

        await _paymentRepository.CreatePaymentAsync(payment);
        
        await _idempotencyServiceContract.CompleteAsync(
            userId,
            idempotencyKey,
            payment.Id,
            IdempotencyOperation.Payment);
        
        await _unitOfWorkContract.SaveAsync();
        await Task.Delay(5000);
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