using Application.Common.Interfaces;
using Application.Features.Cart.DTOs;
using Application.Features.Cart.Interfaces;
using Application.Features.CartItem.Interfaces;
using Application.Features.Inventory.Interfaces;
using Domain.Enums;
using Shared.Exceptions;

namespace Application.Features.Cart.implementations;

public class CartService : CartServicesContract
{
    private readonly CartRepositoryContract _cartRepository;
    private readonly CartItemRepositoryContract _cartItemRepositoryContract;
    private readonly UnitOfWorkContract _unitOfWorkContract;
    private readonly InventoryRepositoryContract _inventoryRepositoryContract;
    private readonly IUSerContext _userContext;

    public CartService(CartRepositoryContract cartRepository, IUSerContext userContext,
        InventoryRepositoryContract inventoryRepositoryContract, UnitOfWorkContract unitOfWorkContract,
        CartItemRepositoryContract cartItemRepositoryContract)
    {
        _cartRepository = cartRepository;
        _userContext = userContext;
        _inventoryRepositoryContract = inventoryRepositoryContract;
        _unitOfWorkContract = unitOfWorkContract;
        _cartItemRepositoryContract = cartItemRepositoryContract;
    }

    public async Task<string> AddItemAsync(AddCartItemDto dto)
    {
        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        if (dto.Quantity <= 0)
            throw new InvalidQuantityException(
                "تعداد درخواستی باید بیشتر از صفر باشد.");

        var inventory = await _inventoryRepositoryContract
            .GetByProductIdAsync(dto.ProductId);

        if (inventory is null)
            throw new NotFoundException("محصول یافت نشد.");

        var cart = await _cartRepository
            .GetCartByUserIdAsync(userId);

        if (cart is null)
        {
            cart = new Domain.Entities.Cart(userId);

            await _cartRepository.CreateCartAsync(cart);
        }

        var existingItem = cart.CartItems
            .FirstOrDefault(x => x.ProductId == dto.ProductId);

        var requestedQuantity = existingItem is null
            ? dto.Quantity
            : existingItem.Quantity + dto.Quantity;

        if (requestedQuantity > inventory.AvailableQuantity)
        {
            throw new InsufficientStockException(
                "مجموع تعداد درخواستی در سبد خرید، از موجودی انبار بیشتر است.");
        }

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(dto.Quantity);
        }
        else
        {
            var cartItem = new Domain.Entities.CartItem(
                cart.Id,
                dto.ProductId,
                dto.Quantity);

            await _cartItemRepositoryContract.AddCartItemAsync(cartItem);
        }

        await _unitOfWorkContract.SaveAsync();

        return "با موفقیت به سبد خرید اضافه شد";
    }

    public async Task<string> UpdateItemQuantityAsync(UpdateCartDto dto)
    {
        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        if (dto.NewQuantity <= 0)
            throw new InvalidQuantityException(
                "تعداد باید بیشتر از صفر باشد. برای حذف، از متد حذف استفاده کنید.");

        var cart = await _cartRepository
            .GetCartByUserIdAsync(userId);

        if (cart is null)
            throw new NotFoundException("سبد خرید یافت نشد.");

        var inventory = await _inventoryRepositoryContract
            .GetByProductIdAsync(dto.ProductId);

        if (inventory is null)
            throw new NotFoundException("محصول یافت نشد.");

        if (dto.NewQuantity > inventory.AvailableQuantity)
        {
            throw new InsufficientStockException(
                $"موجودی انبار کافی نیست. حداکثر موجودی: {inventory.AvailableQuantity}");
        }

        cart.UpdateItemQuantity(
            dto.ProductId,
            dto.NewQuantity);

        await _unitOfWorkContract.SaveAsync();
        return "تعداد با موفقیت بروزرسانی شد .";
    }

    public async Task<ViewCartDto> GetCartByUserIdAsync()
    {
        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        var cart = await _cartRepository
            .GetCartWithProductsByUserIdAsync(userId);

        if (cart is null)
        {
            return new ViewCartDto
            {
                Id = null,
                UserId = userId,
                Items = []
            };
        }

        var now = DateTime.UtcNow;

        var items = cart.CartItems.Select(x =>
        {
            var activeDiscount = x.Product.DiscountProducts
                .Select(dp => dp.Discount)
                .FirstOrDefault(d =>
                    !d.IsDeleted &&
                    d.IsActive &&
                    d.StartsAt <= now &&
                    d.EndsAt > now);


            long finalPrice = x.Product.Price;
            long discountAmount = 0;
            decimal? discountPercentage = null;


            if (activeDiscount is not null)
            {
                if (activeDiscount.DiscountType == DiscountType.Percentage)
                {
                    discountPercentage = activeDiscount.Value;

                    discountAmount =
                        (long)(x.Product.Price *
                               (activeDiscount.Value / 100));

                    if (activeDiscount.MaxDiscountAmount.HasValue)
                    {
                        discountAmount = Math.Min(
                            discountAmount,
                            (long)activeDiscount.MaxDiscountAmount.Value);
                    }

                    finalPrice = x.Product.Price - discountAmount;
                }
                else if (activeDiscount.DiscountType == DiscountType.FixedAmount)
                {
                    discountAmount = (long)activeDiscount.Value;

                    finalPrice = Math.Max(
                        0,
                        x.Product.Price - discountAmount);
                }
            }


            return new ViewCartItemsDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductTitle = x.Product.Title,

                Quantity = x.Quantity,

                UnitPrice = x.Product.Price,
                FinalPrice = finalPrice,
                DiscountAmount = discountAmount,
                DiscountPercentage = discountPercentage
            };
        }).ToList();


        return new ViewCartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = items
        };
    }

    public async Task<string> DeleteItemAsync(Guid productId)
    {
        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        var cart = await _cartRepository
            .GetCartByUserIdAsync(userId);

        if (cart is null)
            throw new NotFoundException("سبد خرید یافت نشد.");

        var item = cart.CartItems
            .FirstOrDefault(x => x.ProductId == productId);

        if (item is null)
            throw new NotFoundException(
                "محصول در سبد خرید یافت نشد.");

        cart.RemoveCartItem(item);

        await _unitOfWorkContract.SaveAsync();
        return "محصول با موفقیت حذف شد .";
    }

    public async Task<string> ClearCartAsync()
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart is null) throw new Exception("سبد خرید یافت نشد");

        cart.ClearCart();
        await _unitOfWorkContract.SaveAsync();
        return "سبد خرید کاملاً خالی شد .";
    }

    public async Task<int> GetCartItemsCountAsync()
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart is null) return 0;

        return cart.CartItems.Sum(x => x.Quantity);
    }
}