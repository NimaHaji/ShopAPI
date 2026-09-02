using Application.Common.Interfaces;
using Application.Common.Observability;
using Application.Features.Cart.DTOs;
using Application.Features.Cart.Interfaces;
using Application.Features.CartItem.Interfaces;
using Application.Features.Inventory.Interfaces;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Exceptions;

namespace Application.Features.Cart.implementations;

public class CartService : CartServicesContract
{
    private readonly CartRepositoryContract _cartRepository;
    private readonly CartItemRepositoryContract _cartItemRepositoryContract;
    private readonly UnitOfWorkContract _unitOfWorkContract;
    private readonly InventoryRepositoryContract _inventoryRepositoryContract;
    private readonly IUSerContext _userContext;
    private readonly ILogger<CartService> _logger;

    public CartService(CartRepositoryContract cartRepository, IUSerContext userContext,
        InventoryRepositoryContract inventoryRepositoryContract, UnitOfWorkContract unitOfWorkContract,
        CartItemRepositoryContract cartItemRepositoryContract, ILogger<CartService> logger)
    {
        _cartRepository = cartRepository;
        _userContext = userContext;
        _inventoryRepositoryContract = inventoryRepositoryContract;
        _unitOfWorkContract = unitOfWorkContract;
        _cartItemRepositoryContract = cartItemRepositoryContract;
        _logger = logger;
    }

    public async Task<string> AddItemAsync(AddCartItemDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(AddItemAsync));

        activity?.SetTag("product_variant.id", dto.ProductVariantId);
        activity?.SetTag("cart.quantity", dto.Quantity);

        var userId =
            _userContext.UserId
            ?? throw new UnauthorizedAccessException(
                "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);

        _logger.LogInformation(
            LogEvents.Cart.AddItemStarted,
            "Adding item to cart started. ProductVariantId: {ProductVariantId}, Quantity: {Quantity}",
            dto.ProductVariantId,
            dto.Quantity);

        if (dto.Quantity <= 0)
        {
            _logger.LogWarning(
                LogEvents.Cart.InvalidQuantity,
                "Adding item to cart rejected because quantity is invalid. ProductVariantId: {ProductVariantId}, Quantity: {Quantity}",
                dto.ProductVariantId,
                dto.Quantity);

            throw new InvalidQuantityException(
                "تعداد درخواستی باید بیشتر از صفر باشد.");
        }

        var inventory =
            await _inventoryRepositoryContract
                .GetByProductVariantIdAsync(dto.ProductVariantId);

        if (inventory is null)
        {
            _logger.LogWarning(
                LogEvents.Cart.ProductNotFound,
                "Adding item to cart rejected because the product variant was not found. ProductVariantId: {ProductVariantId}",
                dto.ProductVariantId);

            throw new NotFoundException("محصول یافت نشد.");
        }

        var cart =
            await _cartRepository
                .GetCartByUserIdAsync(userId);

        if (cart is null)
        {
            cart = new Domain.Entities.Cart(userId);

            await _cartRepository.CreateCartAsync(cart);
        }

        var existingItem = cart.CartItems
            .FirstOrDefault(x =>
                x.ProductVariantId == dto.ProductVariantId);

        var requestedQuantity = existingItem is null
            ? dto.Quantity
            : existingItem.Quantity + dto.Quantity;

        if (requestedQuantity > inventory.AvailableQuantity)
        {
            _logger.LogWarning(
                LogEvents.Cart.InsufficientStock,
                "Adding item to cart rejected because requested quantity exceeds available stock. ProductVariantId: {ProductVariantId}, RequestedQuantity: {RequestedQuantity}, AvailableQuantity: {AvailableQuantity}",
                dto.ProductVariantId,
                requestedQuantity,
                inventory.AvailableQuantity);

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
                dto.ProductVariantId,
                dto.Quantity);

            await _cartItemRepositoryContract
                .AddCartItemAsync(cartItem);
        }

        await _unitOfWorkContract.SaveAsync();

        activity?.SetTag("cart.id", cart.Id);
        activity?.SetTag("cart.requested_quantity", requestedQuantity);

        _logger.LogInformation(
            LogEvents.Cart.AddItemCompleted,
            "Item added to cart successfully. CartId: {CartId}, ProductVariantId: {ProductVariantId}",
            cart.Id,
            dto.ProductVariantId);

        return "با موفقیت به سبد خرید اضافه شد";
    }

    public async Task<string> UpdateItemQuantityAsync(UpdateCartDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(UpdateItemQuantityAsync));

        activity?.SetTag("product_variant.id", dto.ProductVariantId);
        activity?.SetTag("cart.quantity", dto.NewQuantity);

        var userId =
            _userContext.UserId
            ?? throw new UnauthorizedAccessException(
                "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);

        _logger.LogInformation(
            LogEvents.Cart.UpdateItemStarted,
            "Cart item quantity update started. ProductVariantId: {ProductVariantId}, NewQuantity: {NewQuantity}",
            dto.ProductVariantId,
            dto.NewQuantity);

        if (dto.NewQuantity <= 0)
        {
            _logger.LogWarning(
                LogEvents.Cart.InvalidQuantity,
                "Cart item quantity update rejected because quantity is invalid. ProductVariantId: {ProductVariantId}, Quantity: {Quantity}",
                dto.ProductVariantId,
                dto.NewQuantity);

            throw new InvalidQuantityException(
                "تعداد باید بیشتر از صفر باشد. برای حذف، از متد حذف استفاده کنید.");
        }

        var cart =
            await _cartRepository
                .GetCartByUserIdAsync(userId);

        if (cart is null)
        {
            _logger.LogWarning(
                LogEvents.Cart.NotFound,
                "Cart was not found. UserId: {UserId}",
                userId);

            throw new NotFoundException(
                "سبد خرید یافت نشد.");
        }

        var inventory =
            await _inventoryRepositoryContract
                .GetByProductVariantIdAsync(dto.ProductVariantId);

        if (inventory is null)
        {
            _logger.LogWarning(
                LogEvents.Cart.ProductNotFound,
                "Cart item quantity update rejected because the product variant was not found. ProductVariantId: {ProductVariantId}",
                dto.ProductVariantId);

            throw new NotFoundException(
                "محصول یافت نشد.");
        }

        if (dto.NewQuantity > inventory.AvailableQuantity)
        {
            _logger.LogWarning(
                LogEvents.Cart.InsufficientStock,
                "Cart item quantity update rejected because requested quantity exceeds available stock. ProductVariantId: {ProductVariantId}, RequestedQuantity: {RequestedQuantity}, AvailableQuantity: {AvailableQuantity}",
                dto.ProductVariantId,
                dto.NewQuantity,
                inventory.AvailableQuantity);

            throw new InsufficientStockException(
                $"موجودی انبار کافی نیست. حداکثر موجودی: {inventory.AvailableQuantity}");
        }

        cart.UpdateItemQuantity(
            dto.ProductVariantId,
            dto.NewQuantity);

        await _unitOfWorkContract.SaveAsync();

        activity?.SetTag("cart.id", cart.Id);

        _logger.LogInformation(
            LogEvents.Cart.UpdateItemCompleted,
            "Cart item quantity updated successfully. CartId: {CartId}, ProductVariantId: {ProductVariantId}",
            cart.Id,
            dto.ProductVariantId);

        return "تعداد با موفقیت بروزرسانی شد .";
    }

    public async Task<ViewCartDto> GetCartByUserIdAsync()
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(GetCartByUserIdAsync));

        var userId =
            _userContext.UserId
            ?? throw new UnauthorizedAccessException(
                "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);

        var cart =
            await _cartRepository
                .GetCartWithProductsByUserIdAsync(userId);

        if (cart is null)
        {
            activity?.SetTag("cart.items_count", 0);

            return new ViewCartDto
            {
                Id = null,
                UserId = userId,
                Items = []
            };
        }

        var now = DateTime.UtcNow;

        var items = cart.CartItems
            .Select(x =>
            {
                var variant = x.ProductVariant;

                var variantDiscount = variant.DiscountVariants
                    .Select(dv => dv.Discount)
                    .FirstOrDefault(d =>
                        !d.IsDeleted &&
                        d.IsActive &&
                        d.StartsAt <= now &&
                        d.EndsAt > now);

                var productDiscount = variant.Product
                    .DiscountProducts
                    .Select(dp => dp.Discount)
                    .FirstOrDefault(d =>
                        !d.IsDeleted &&
                        d.IsActive &&
                        d.StartsAt <= now &&
                        d.EndsAt > now);

                var activeDiscount =
                    variantDiscount ?? productDiscount;

                long unitPrice = variant.Price;
                long discountAmount = 0;
                decimal? discountPercentage = null;

                if (activeDiscount is not null)
                {
                    if (activeDiscount.DiscountType ==
                        DiscountType.Percentage)
                    {
                        discountPercentage =
                            activeDiscount.Value;

                        discountAmount = (long)(
                            unitPrice *
                            (activeDiscount.Value / 100));

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

                var finalPrice =
                    Math.Max(0, unitPrice - discountAmount);

                return new ViewCartItemDto
                {
                    Id = x.Id,
                    ProductId = variant.ProductId,
                    ProductVariantId = variant.Id,
                    ProductTitle = variant.Product.Title,
                    VariantSku = variant.Sku,
                    Quantity = x.Quantity,
                    UnitPrice = unitPrice,
                    FinalPrice = finalPrice,
                    DiscountAmount = discountAmount,
                    DiscountPercentage = discountPercentage
                };
            })
            .ToList();

        activity?.SetTag("cart.id", cart.Id);
        activity?.SetTag("cart.items_count", items.Count);

        return new ViewCartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = items
        };
    }

    public async Task<string> DeleteItemAsync(Guid productVariantId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(DeleteItemAsync));

        activity?.SetTag("product_variant.id", productVariantId);

        var userId =
            _userContext.UserId
            ?? throw new UnauthorizedAccessException(
                "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);

        _logger.LogInformation(
            LogEvents.Cart.DeleteItemStarted,
            "Cart item deletion started. ProductVariantId: {ProductVariantId}",
            productVariantId);

        var cart =
            await _cartRepository
                .GetCartByUserIdAsync(userId);

        if (cart is null)
        {
            _logger.LogWarning(
                LogEvents.Cart.NotFound,
                "Cart was not found. UserId: {UserId}",
                userId);

            throw new NotFoundException(
                "سبد خرید یافت نشد.");
        }

        var item = cart.CartItems
            .FirstOrDefault(x =>
                x.ProductVariantId == productVariantId);

        if (item is null)
        {
            _logger.LogWarning(
                LogEvents.Cart.ItemNotFound,
                "Cart item was not found. CartId: {CartId}, ProductVariantId: {ProductVariantId}",
                cart.Id,
                productVariantId);

            throw new NotFoundException(
                "محصول در سبد خرید یافت نشد.");
        }

        cart.RemoveCartItem(item);

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Cart.DeleteItemCompleted,
            "Cart item deleted successfully. CartId: {CartId}, ProductVariantId: {ProductVariantId}",
            cart.Id,
            productVariantId);

        return "محصول با موفقیت حذف شد .";
    }

    public async Task<string> ClearCartAsync()
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(ClearCartAsync));

        var userId =
            _userContext.UserId
            ?? throw new UnauthorizedAccessException(
                "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);

        _logger.LogInformation(
            LogEvents.Cart.ClearStarted,
            "Cart clearing started. UserId: {UserId}",
            userId);

        var cart =
            await _cartRepository
                .GetCartByUserIdAsync(userId);

        if (cart is null)
        {
            _logger.LogWarning(
                LogEvents.Cart.NotFound,
                "Cart was not found for clearing. UserId: {UserId}",
                userId);

            throw new NotFoundException(
                "سبد خرید یافت نشد");
        }

        cart.ClearCart();

        await _unitOfWorkContract.SaveAsync();

        activity?.SetTag("cart.id", cart.Id);

        _logger.LogInformation(
            LogEvents.Cart.ClearCompleted,
            "Cart cleared successfully. CartId: {CartId}",
            cart.Id);

        return "سبد خرید کاملاً خالی شد .";
    }

    public async Task<int> GetCartItemsCountAsync()
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(GetCartItemsCountAsync));

        var userId =
            _userContext.UserId
            ?? throw new UnauthorizedAccessException(
                "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);

        var cart =
            await _cartRepository
                .GetCartByUserIdAsync(userId);

        if (cart is null)
        {
            activity?.SetTag("cart.items_count", 0);
            return 0;
        }

        var count = cart.CartItems.Sum(x => x.Quantity);

        activity?.SetTag("cart.id", cart.Id);
        activity?.SetTag("cart.items_count", count);

        return count;
    }
}