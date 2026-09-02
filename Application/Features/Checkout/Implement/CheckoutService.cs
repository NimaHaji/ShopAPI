using System.Diagnostics;
using Application.Common.Interfaces;
using Application.Common.Observability;
using Application.Features.Address.Interfaces;
using Application.Features.Cart.Interfaces;
using Application.Features.Checkout.DTOs;
using Application.Features.Checkout.Interfaces;
using Application.Features.Coupon.DTOs;
using Application.Features.Coupon.Interfaces;
using Application.Features.IdempotencyKey.DTOs;
using Application.Features.IdempotencyKey.Interfaces;
using Application.Features.Inventory.Interfaces;
using Application.Features.Order.DTOs;
using Application.Features.Order.Interfaces;
using Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Exceptions;

namespace Application.Features.Checkout.Implement;

public class CheckoutService : CheckoutServiceContract
{
    private readonly CartRepositoryContract _cartRepositoryContract;
    private readonly CouponsServiceContract _couponsServiceContract;
    private readonly OrderServicesContract _orderServicesContract;
    private readonly InventoryServiceContract _inventoryServiceContract;
    private readonly UnitOfWorkContract _unitOfWork;
    private readonly IUSerContext _userContext;
    private readonly AddressRepositoryContract _addressRepositoryContract;
    private readonly IdempotencyServiceContract _idempotencyServiceContract;
    private readonly ILogger<CheckoutService> _logger;

    public CheckoutService(CartRepositoryContract cartRepositoryContract, UnitOfWorkContract unitOfWork,
        IUSerContext userContext,
        InventoryServiceContract inventoryServiceContract, OrderServicesContract orderServicesContract,
        CouponsServiceContract couponsServiceContract, AddressRepositoryContract addressRepositoryContract,
        IdempotencyServiceContract idempotencyServiceContract, ILogger<CheckoutService> logger)
    {
        _cartRepositoryContract = cartRepositoryContract;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _inventoryServiceContract = inventoryServiceContract;
        _orderServicesContract = orderServicesContract;
        _couponsServiceContract = couponsServiceContract;
        _addressRepositoryContract = addressRepositoryContract;
        _idempotencyServiceContract = idempotencyServiceContract;
        _logger = logger;
    }

    public async Task<CheckoutResultDto> CheckoutAsync(
        CheckoutDto dto,
        string idempotencyKey)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(CheckoutAsync));

        int attempts = 0;
        const int maxAttempts = 4;

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);
        activity?.SetTag("address.id", dto.AddressId);
        activity?.SetTag(
            "checkout.has_coupon",
            !string.IsNullOrWhiteSpace(dto.CouponCode));

        _logger.LogInformation(
            LogEvents.Checkout.Started,
            "Checkout request started. AddressId: {AddressId}, HasCoupon: {HasCoupon}",
            dto.AddressId,
            !string.IsNullOrWhiteSpace(dto.CouponCode));

        var userAddress =
            await _addressRepositoryContract
                .GetAddressByIdAndUserIdAsync(userId, dto.AddressId);

        if (userAddress is null)
        {
            _logger.LogWarning(
                LogEvents.Checkout.AddressNotFound,
                "Checkout rejected because the address was not found. AddressId: {AddressId}",
                dto.AddressId);

            throw new NotFoundException(
                "آدرس انتخاب شده یافت نشد.");
        }

        while (attempts < maxAttempts)
        {
            ValidateCouponResponseDto? couponResult = null;

            if (!string.IsNullOrWhiteSpace(dto.CouponCode))
            {
                couponResult =
                    await _couponsServiceContract.ValidateCouponAsync(
                        new ValidateCouponDto
                        {
                            Code = dto.CouponCode
                        });
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var idempotencyResult =
                    await _idempotencyServiceContract.CheckAsync(
                        userId,
                        idempotencyKey,
                        IdempotencyOperation.Checkout);

                if (idempotencyResult.Status ==
                    IdempotencyStatusDto.Completed)
                {
                    _logger.LogInformation(
                        LogEvents.Checkout.IdempotencyCompleted,
                        "Checkout request was already completed. OrderId: {OrderId}",
                        idempotencyResult.ResourceId);

                    await _unitOfWork.RollbackTransactionAsync();

                    return new CheckoutResultDto
                    {
                        OrderId = idempotencyResult.ResourceId!.Value
                    };
                }

                if (idempotencyResult.Status ==
                    IdempotencyStatusDto.Processing)
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    _logger.LogWarning(
                        LogEvents.Checkout.IdempotencyConflict,
                        "Checkout rejected because another request with the same idempotency key is being processed.");

                    throw new ConflictException(
                        "درخواست دیگری با همین کلید جلوگیری از ثبت تکراری (Idempotency-Key) در حال پردازش است.");
                }

                var cart =
                    await _cartRepositoryContract
                        .GetCartWithProductsByUserIdAsync(userId);

                if (cart is null)
                {
                    _logger.LogWarning(
                        LogEvents.Checkout.CartNotFound,
                        "Checkout rejected because the cart was not found.");

                    throw new NotFoundException(
                        "سبد خرید پیدا نشد");
                }

                if (cart.CartItems is null || !cart.CartItems.Any())
                {
                    _logger.LogWarning(
                        LogEvents.Checkout.Rejected,
                        "Checkout rejected because the cart is empty. CartId: {CartId}",
                        cart.Id);

                    throw new CartEmptyException(
                        "سبد خرید خالی است");
                }

                await _unitOfWork.SaveAsync();

                using var reserveStockActivity =
                    ActivitySources.ShopApi.StartActivity(
                        "Checkout.ReserveStock");

                reserveStockActivity?.SetTag(
                    "checkout.items_count",
                    cart.CartItems.Count);

                try
                {
                    await _inventoryServiceContract
                        .ReserveAllItemStockAsync(cart.CartItems);

                    reserveStockActivity?.SetStatus(
                        ActivityStatusCode.Ok);
                }
                catch (Exception ex)
                {
                    reserveStockActivity?.SetStatus(
                        ActivityStatusCode.Error,
                        ex.Message);

                    reserveStockActivity?.AddEvent(
                        new ActivityEvent(
                            "exception",
                            tags: new ActivityTagsCollection
                            {
                                ["exception.type"] = ex.GetType().FullName,
                                ["exception.message"] = ex.Message,
                                ["exception.stacktrace"] = ex.StackTrace
                            }));

                    throw;
                }

                var createOrderDto = new CreateOrderDto
                {
                    Items = cart.CartItems
                        .Select(x => new OrderItemDto
                        {
                            ProductVariantId = x.ProductVariantId,
                            Quantity = x.Quantity
                        })
                        .ToList(),

                    CouponCode = couponResult?.Code,
                    CouponDiscountAmount =
                        couponResult?.DiscountAmount ?? 0,
                    CouponId = couponResult?.CouponId
                };

                var orderId =
                    await _orderServicesContract
                        .CreateOrderAsync(
                            createOrderDto,
                            userAddress);

                cart.ClearCart();

                await _idempotencyServiceContract.CompleteAsync(
                    userId,
                    idempotencyKey,
                    orderId,
                    IdempotencyOperation.Checkout);

                await _unitOfWork.SaveAsync();

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    LogEvents.Checkout.Completed,
                    "Checkout completed successfully. OrderId: {OrderId}, CartId: {CartId}",
                    orderId,
                    cart.Id);

                return new CheckoutResultDto
                {
                    OrderId = orderId
                };
            }
            catch (DbUpdateConcurrencyException)
            {
                attempts++;

                await _unitOfWork.RollbackTransactionAsync();
                _unitOfWork.ClearChangeTracker();

                _logger.LogWarning(
                    LogEvents.Checkout.ConcurrencyConflict,
                    "Checkout concurrency conflict occurred. Attempt: {Attempt}, MaxAttempts: {MaxAttempts}",
                    attempts,
                    maxAttempts);

                if (attempts == maxAttempts)
                {
                    _logger.LogError(
                        LogEvents.Checkout.Failed,
                        "Checkout failed after maximum concurrency retry attempts. Attempts: {Attempts}",
                        attempts);

                    throw new ConflictException(
                        "موجودی در حال تغییر است. لطفاً دوباره تلاش کنید.");
                }
            }
            catch (DbUpdateException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _unitOfWork.ClearChangeTracker();

                if (ex.InnerException is SqlException sqlException &&
                    sqlException.Number is 2601 or 2627)
                {
                    _logger.LogWarning(
                        LogEvents.Checkout.IdempotencyConflict,
                        "Checkout idempotency conflict detected. SqlErrorNumber: {SqlErrorNumber}",
                        sqlException.Number);

                    var existing =
                        await _idempotencyServiceContract.CheckAsync(
                            userId,
                            idempotencyKey,
                            IdempotencyOperation.Checkout);

                    if (existing.Status ==
                        IdempotencyStatusDto.Completed)
                    {
                        _logger.LogInformation(
                            LogEvents.Checkout.IdempotencyCompleted,
                            "Checkout was completed by another request. OrderId: {OrderId}",
                            existing.ResourceId);

                        return new CheckoutResultDto
                        {
                            OrderId = existing.ResourceId!.Value
                        };
                    }

                    throw new ConflictException(
                        "درخواست دیگری با همین Idempotency-Key در حال پردازش است.");
                }

                _logger.LogError(
                    LogEvents.Checkout.Failed,
                    ex,
                    "Database update failed during checkout.");

                throw;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogError(
                    LogEvents.Checkout.Failed,
                    ex,
                    "Unexpected error occurred during checkout.");

                throw;
            }
        }

        throw new InvalidOperationException(
            "خطای ناشناخته");
    }
}