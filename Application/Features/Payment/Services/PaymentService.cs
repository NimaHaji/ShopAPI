using System.Diagnostics;
using Application.Common.Interfaces;
using Application.Common.Observability;
using Application.Features.Coupon.Interfaces;
using Application.Features.CouponUsage.Interfaces;
using Application.Features.IdempotencyKey.DTOs;
using Application.Features.IdempotencyKey.Interfaces;
using Application.Features.Order.Interfaces;
using Application.Features.Payment.DTOs;
using Application.Features.Payment.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Shared;
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
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(PaymentRepositoryContract paymentRepository, PaymentGatewayResolverContract gatewayResolver,
        OrderRepositoryContract orderRepository, IUSerContext userContext, UnitOfWorkContract unitOfWorkContract,
        CouponUsageRepositoryContract couponUsageRepositoryContract, CouponRepositoryContract couponRepositoryContract,
        IdempotencyServiceContract idempotencyServiceContract, ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _gatewayResolver = gatewayResolver;
        _orderRepository = orderRepository;
        _userContext = userContext;
        _unitOfWorkContract = unitOfWorkContract;
        _couponUsageRepositoryContract = couponUsageRepositoryContract;
        _couponRepositoryContract = couponRepositoryContract;
        _idempotencyServiceContract = idempotencyServiceContract;
        _logger = logger;
    }

    public async Task<string?> CreatePaymentAsync(
        CreatePaymentDto dto,
        string idempotencyKey)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(CreatePaymentAsync));

        activity?.SetTag("order.id", dto.OrderId);
        activity?.SetTag("payment.gateway", dto.Gateway);

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);

        _logger.LogInformation(
            LogEvents.Payment.CreateStarted,
            "Payment creation started. OrderId: {OrderId}, Gateway: {Gateway}",
            dto.OrderId,
            dto.Gateway);

        var existing =
            await _idempotencyServiceContract.CheckAsync(
                userId,
                idempotencyKey,
                IdempotencyOperation.Payment);

        if (existing.Status == IdempotencyStatusDto.Processing)
        {
            _logger.LogWarning(
                LogEvents.Payment.IdempotencyConflict,
                "Payment creation rejected because another request with the same idempotency key is being processed. OrderId: {OrderId}",
                dto.OrderId);

            throw new ConflictException(
                "درخواست دیگری با همین Idempotency-Key در حال پردازش است.");
        }

        if (existing.Status == IdempotencyStatusDto.Completed)
        {
            if (existing.ResourceId.HasValue)
            {
                var previousPayment =
                    await _paymentRepository
                        .GetPaymentByIdAsync(existing.ResourceId.Value);

                if (previousPayment is null)
                {
                    _logger.LogError(
                        LogEvents.Payment.Failed,
                        "Previous payment referenced by idempotency record was not found. PaymentId: {PaymentId}",
                        existing.ResourceId.Value);

                    throw new InvalidOperationException(
                        "پرداخت مربوط به درخواست قبلی یافت نشد.");
                }

                _logger.LogInformation(
                    LogEvents.Payment.IdempotencyCompleted,
                    "Payment request was already completed. PaymentId: {PaymentId}, OrderId: {OrderId}",
                    previousPayment.Id,
                    dto.OrderId);

                activity?.SetTag(
                    "payment.id",
                    previousPayment.Id);

                return previousPayment.PaymentUrl;
            }
        }

        await _unitOfWorkContract.SaveAsync();

        var order =
            await _orderRepository.GetOrderByIdAsync(
                dto.OrderId,
                userId);

        if (order is null)
        {
            _logger.LogWarning(
                LogEvents.Payment.OrderNotFound,
                "Payment creation rejected because the order was not found. OrderId: {OrderId}",
                dto.OrderId);

            throw new NotFoundException(
                "سفارشی یافت نشد");
        }

        var payment =
            new Domain.Entities.Payment(
                order.TotalPrice,
                dto.Description,
                dto.Gateway,
                order.Id);

        var provider =
            _gatewayResolver.Resolve(dto.Gateway);

        var requestResult =
            await provider.RequestPaymentAsync(
                payment,
                dto);

        if (!requestResult.IsSuccess)
        {
            payment.MarkAsFailed();

            await _unitOfWorkContract.SaveAsync();

            _logger.LogWarning(
                LogEvents.Payment.GatewayFailed,
                "Payment gateway request failed. OrderId: {OrderId}, Gateway: {Gateway}",
                order.Id,
                dto.Gateway);

            throw new InvalidOperationException(
                requestResult.ErrorMessage);
        }

        await _paymentRepository.CreatePaymentAsync(payment);

        await _idempotencyServiceContract.CompleteAsync(
            userId,
            idempotencyKey,
            payment.Id,
            IdempotencyOperation.Payment);

        await _unitOfWorkContract.SaveAsync();

        activity?.SetTag(
            "payment.id",
            payment.Id);

        activity?.SetStatus(
            ActivityStatusCode.Ok);

        _logger.LogInformation(
            LogEvents.Payment.Created,
            "Payment created successfully. PaymentId: {PaymentId}, OrderId: {OrderId}, Gateway: {Gateway}",
            payment.Id,
            order.Id,
            dto.Gateway);

        return requestResult.PaymentUrl;
    }

    public async Task<VerifyPaymentResult> HandleCallBackAsync(
        PaymentGateway gateway,
        SandBoxCallBackDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(HandleCallBackAsync));

        activity?.SetTag(
            "payment.gateway",
            gateway.ToString());

        _logger.LogInformation(
            LogEvents.Payment.CallbackStarted,
            "Payment callback received. Gateway: {Gateway}",
            gateway);

        var provider =
            _gatewayResolver.Resolve(gateway);

        var result =
            await provider.HandleCallBackAsync(dto);

        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                LogEvents.Payment.GatewayFailed,
                "Payment gateway callback verification failed. Gateway: {Gateway}",
                gateway);

            return result;
        }

        var payment =
            await _paymentRepository
                .GetPaymentByAuthorityAsync(dto.Authority);

        if (payment is null)
        {
            _logger.LogWarning(
                LogEvents.Payment.TransactionNotFound,
                "Payment transaction was not found for callback.");

            return VerifyPaymentResult.Failed(
                "تراکنش یافت نشد.");
        }

        activity?.SetTag(
            "payment.id",
            payment.Id);

        activity?.SetTag(
            "order.id",
            payment.OrderId);

        var order =
            await _orderRepository
                .GetOrderByIdAsync(payment.OrderId);

        if (order is null)
        {
            _logger.LogWarning(
                LogEvents.Payment.OrderNotFound,
                "Order associated with payment was not found. PaymentId: {PaymentId}, OrderId: {OrderId}",
                payment.Id,
                payment.OrderId);

            return VerifyPaymentResult.Failed(
                "سفارش مرتبط با پرداخت یافت نشد.");
        }

        try
        {
            await _unitOfWorkContract
                .BeginTransactionAsync();

            if (order.OrderStatus == OrderStatus.Paid)
            {
                _logger.LogInformation(
                    LogEvents.Payment.AlreadyPaid,
                    "Payment callback ignored because order is already paid. OrderId: {OrderId}, PaymentId: {PaymentId}",
                    order.Id,
                    payment.Id);

                await _unitOfWorkContract
                    .CommitTransactionAsync();

                return result;
            }

            order.ChangeOrderStatusTo(
                OrderStatus.Paid);

            if (order.CouponId.HasValue)
            {
                var coupon =
                    await _couponRepositoryContract
                        .GetCouponByIdAsync(
                            order.CouponId.Value);

                if (coupon is null)
                    throw new NotFoundException(
                        "کد تخفیف یافت نشد.");

                var isExistCouponUsage =
                    await _couponUsageRepositoryContract
                        .IsExistCouponUsageByOrderId(
                            order.Id);

                if (!isExistCouponUsage)
                {
                    var couponUsage =
                        new Domain.Entities.CouponUsage(
                            couponId: order.CouponId.Value,
                            userId: order.UserId,
                            orderId: order.Id,
                            discountAmount:
                            order.CouponDiscountAmount);

                    coupon.IncreaseUsage();

                    await _couponUsageRepositoryContract
                        .CreateCouponUsage(couponUsage);
                }
            }

            await _unitOfWorkContract.SaveAsync();

            await _unitOfWorkContract
                .CommitTransactionAsync();

            activity?.SetStatus(
                ActivityStatusCode.Ok);

            _logger.LogInformation(
                LogEvents.Payment.PaymentCompleted,
                "Payment callback processed successfully. PaymentId: {PaymentId}, OrderId: {OrderId}",
                payment.Id,
                order.Id);

            return result;
        }
        catch (Exception ex)
        {
            await _unitOfWorkContract
                .RollbackTransactionAsync();

            _logger.LogError(
                LogEvents.Payment.Failed,
                ex,
                "Payment callback processing failed. PaymentId: {PaymentId}, OrderId: {OrderId}",
                payment.Id,
                payment.OrderId);

            throw;
        }
    }
}