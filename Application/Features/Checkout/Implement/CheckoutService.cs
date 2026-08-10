using Application.Common.Interfaces;
using Application.Features.Address.Interfaces;
using Application.Features.Cart.Interfaces;
using Application.Features.Checkout.DTOs;
using Application.Features.Checkout.Interfaces;
using Application.Features.Coupon.DTOs;
using Application.Features.Coupon.Interfaces;
using Application.Features.IdempotencyKey.Interfaces;
using Application.Features.Inventory.Interfaces;
using Application.Features.Order.DTOs;
using Application.Features.Order.Interfaces;
using Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

    public CheckoutService(CartRepositoryContract cartRepositoryContract, UnitOfWorkContract unitOfWork,
        IUSerContext userContext,
        InventoryServiceContract inventoryServiceContract, OrderServicesContract orderServicesContract,
        CouponsServiceContract couponsServiceContract, AddressRepositoryContract addressRepositoryContract,
        IdempotencyServiceContract idempotencyServiceContract)
    {
        _cartRepositoryContract = cartRepositoryContract;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _inventoryServiceContract = inventoryServiceContract;
        _orderServicesContract = orderServicesContract;
        _couponsServiceContract = couponsServiceContract;
        _addressRepositoryContract = addressRepositoryContract;
        _idempotencyServiceContract = idempotencyServiceContract;
    }

    public async Task<CheckoutResultDto> CheckoutAsync(CheckoutDto dto, string idempotencyKey)
    {
        int attempts = 0;
        const int maxAttempts = 4;

        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");

        var userAddress = await _addressRepositoryContract.GetAddressByIdAndUserIdAsync(userId, dto.AddressId);

        if (userAddress is null)
            throw new NotFoundException("آدرس انتخاب شده یافت نشد .");

        while (attempts < maxAttempts)
        {

            ValidateCouponResponseDto? couponResult = null;

            if (!string.IsNullOrWhiteSpace(dto.CouponCode))
            {
                couponResult = await _couponsServiceContract
                    .ValidateCouponAsync(new ValidateCouponDto
                    {
                        Code = dto.CouponCode
                    });
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var idempotencyResult = await _idempotencyServiceContract.CheckAsync(userId, idempotencyKey);

                if (idempotencyResult.Status == IdempotencyStatus.Completed)
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    return new CheckoutResultDto
                    {
                        OrderId = idempotencyResult.OrderId
                    };
                }

                if (idempotencyResult.Status == IdempotencyStatus.Processing)
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    throw new ConflictException(
                        "درخواست دیگری با همین کلید جلوگیری از ثبت تکراری (Idempotency-Key) در حال پردازش است.");
                }
                
                var cart = await _cartRepositoryContract.GetCartWithProductsByUserIdAsync(userId);

                if (cart is null)
                    throw new NotFoundException("سبد خرید پیدا نشد");

                if (cart.CartItems is null || !cart.CartItems.Any())
                    throw new CartEmptyException("سبد خرید خالی است");
                
                await _unitOfWork.SaveAsync();
                await _inventoryServiceContract.ReserveAllItemStockAsync(cart.CartItems);

                var createOrderDto = new CreateOrderDto
                {
                    Items = cart.CartItems.Select(x => new OrderItemDto
                    {
                        ProductId = x.ProductId,
                        Quantity = x.Quantity
                    }).ToList(),
                    CouponCode = couponResult?.Code,
                    CouponDiscountAmount = couponResult?.DiscountAmount ?? 0,
                    CouponId = couponResult?.CouponId
                };
                var orderId = await _orderServicesContract.CreateOrderAsync(createOrderDto, userAddress);
                
                cart.ClearCart();

                await _idempotencyServiceContract.CompleteAsync(
                    userId,
                    idempotencyKey,
                    orderId);

                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();

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
                if (attempts == maxAttempts)
                    throw new ConflictException("موجودی در حال تغییر است. لطفاً دوباره تلاش کنید.");
            }
            catch (DbUpdateException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                
                _unitOfWork.ClearChangeTracker();
                if (ex.InnerException is SqlException sqlException &&
                    sqlException.Number is 2601 or 2627)
                {
                    var existing = await _idempotencyServiceContract
                        .CheckAsync(userId, idempotencyKey);

                    if (existing.Status == IdempotencyStatus.Completed)
                    {
                        return new CheckoutResultDto
                        {
                            OrderId = existing.OrderId
                        };
                    }
                    
                    throw new ConflictException(
                        "درخواست دیگری با همین Idempotency-Key در حال پردازش است.");
                }

                throw;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        throw new InvalidOperationException("خطای ناشناخته");
    }
}