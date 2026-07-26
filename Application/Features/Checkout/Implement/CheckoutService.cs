using System.Data;
using Application.Common.Interfaces;
using Application.Features.Cart.DTOs;
using Application.Features.Cart.Interfaces;
using Application.Features.Checkout.Interfaces;
using Application.Features.Inventory.Interfaces;
using Application.Features.Order.DTOs;
using Application.Features.Order.Interfaces;
using Application.Features.Payment.DTOs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;

namespace Application.Features.Checkout.Implement;

public class CheckoutService : CheckoutServiceContract
{
    private readonly CartRepositoryContract _cartRepositoryContract;
    private readonly OrderRepositoryContract _orderRepositoryContract;
    private readonly OrderServicesContract _orderServicesContract;
    private readonly InventoryServiceContract _inventoryServiceContract;
    private readonly UnitOfWorkContract _unitOfWork;
    private readonly IUSerContext _userContext;

    public CheckoutService(CartRepositoryContract cartRepositoryContract,
        OrderRepositoryContract orderRepositoryContract, UnitOfWorkContract unitOfWork, IUSerContext userContext,
        InventoryServiceContract inventoryServiceContract, OrderServicesContract orderServicesContract)
    {
        _cartRepositoryContract = cartRepositoryContract;
        _orderRepositoryContract = orderRepositoryContract;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _inventoryServiceContract = inventoryServiceContract;
        _orderServicesContract = orderServicesContract;
    }

    public async Task<Guid> CheckoutAsync()
    {
        int attempts = 0;
        const int maxAttempts = 4;

        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");

        while (attempts < maxAttempts)
        {
            var cart = await _cartRepositoryContract.GetCartWithProductsByUserIdAsync(userId);

            if (cart is null)
                throw new NotFoundException("سبد خرید پیدا نشد");

            if (cart.CartItems is null || !cart.CartItems.Any())
                throw new CartEmptyException("سبد خرید خالی است");

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _inventoryServiceContract.ReserveAllItemStockAsync(cart.CartItems);

                var createOrderDto = new CreateOrderDto
                {
                    Items = cart.CartItems.Select(x => new OrderItemDto
                    {
                        ProductId = x.ProductId,
                        Quantity = x.Quantity
                    }).ToList()
                };
                var orderId = await _orderServicesContract.CreateOrderAsync(createOrderDto);


                cart.ClearCart();
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
                return orderId;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // attempts++;
                // await _unitOfWork.RollbackTransactionAsync();
                // _unitOfWork.ClearChangeTracker();
                // if (attempts == maxAttempts)
                //     throw new ConflictException("موجودی در حال تغییر است. لطفاً دوباره تلاش کنید.");
                await _unitOfWork.RollbackTransactionAsync();

                foreach (var entry in ex.Entries)
                {
                    Console.WriteLine(entry.Entity.GetType().Name);
                }

                _unitOfWork.ClearChangeTracker();

                attempts++;

                if (attempts == maxAttempts)
                    throw new ConflictException("موجودی در حال تغییر است");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        throw new InvalidOperationException("خطای ناشناخه");
    }
}