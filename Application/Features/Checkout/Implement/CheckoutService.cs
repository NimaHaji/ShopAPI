using Application.Common.Interfaces;
using Application.Features.Cart.Interfaces;
using Application.Features.Checkout.Interfaces;
using Application.Features.Order.Interfaces;
using Domain.Entities;

namespace Application.Features.Checkout.Implement;

public class CheckoutService:CheckoutServiceContract
{
    private readonly CartRepositoryContract _cartRepositoryContract;
    private readonly OrderRepositoryContract _orderRepositoryContract;
    private readonly UnitOfWorkContract _unitOfWork;
    private readonly IUSerContext _userContext;

    public CheckoutService(CartRepositoryContract cartRepositoryContract, OrderRepositoryContract orderRepositoryContract, UnitOfWorkContract unitOfWork, IUSerContext userContext)
    {
        _cartRepositoryContract = cartRepositoryContract;
        _orderRepositoryContract = orderRepositoryContract;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Guid> CreateOrderFromCartAsync()
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");
        var cart = await _cartRepositoryContract.GetCartWithProductsByUserIdAsync(userId);

        if (cart is null)
            throw new Exception("سبد خرید پیدا نشد");

        if (cart.CartItems is null || !cart.CartItems.Any())
            throw new Exception("سبد خرید خالی است");

        var order = new Domain.Entities.Order
        {
            UserId = userId
        };

        foreach (var cartItem in cart.CartItems)
        {
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = cartItem.ProductId,
                ProductTitle = cartItem.Product.Title,
                Price = cartItem.Product.Price,
                Quantity = cartItem.Quantity
            };

            order.AddItem(orderItem);
        }

        await _orderRepositoryContract.CreateOrderAsync(order);

        cart.ClearCart();
        await _unitOfWork.SaveAsync();

        return order.Id;
    }
}