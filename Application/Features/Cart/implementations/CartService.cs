using Application.Common.Interfaces;
using Application.Features.Cart.DTOs;
using Application.Features.Cart.Interfaces;
using Application.Features.Product.Interfaces;
using Domain.Entities;

namespace Application.Features.Cart.implementations;

public class CartService : CartServicesContract
{
    private readonly CartRepositoryContract _cartRepository;
    private readonly ProductRepositoryContract _productRepository;
    private readonly IUSerContext _userContext;

    public CartService(CartRepositoryContract cartRepository, ProductRepositoryContract productRepository,
        IUSerContext userContext)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _userContext = userContext;
    }

    public async Task<string> AddItemAsync(AddCartItemDto dto)
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");

        if (dto.Quantity <= 0)
        {
            throw new Exception("تعداد درخواستی باید بیشتر از صفر باشد.");
        }

        var product = await _productRepository.GetProductByIdAsync(dto.ProductId);

        if (product is null)
        {
            throw new Exception("محصول مورد نظر یافت نشد.");
        }

        if (product.Stock < dto.Quantity)
        {
            throw new Exception($"موجودی انبار این محصول کافی نیست. موجودی فعلی: {product.Stock}");
        }

        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart is null)
        {
            cart = new Domain.Entities.Cart
            {
                UserId = userId
            };

            await _cartRepository.CreateCartAsync(cart);
        }

        var existingItem = await _cartRepository.GetCartItemByProductIdAsync(product.Id);

        if (existingItem is not null)
        {
            if (product.Stock < (existingItem.Quantity + dto.Quantity))
            {
                throw new Exception("مجموع تعداد درخواستی در سبد خرید، از موجودی انبار بیشتر است.");
            }

            existingItem.Quantity += dto.Quantity;
        }
        else
        {
            cart.CartItems.Add(new CartItem
            {
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            });
        }

        await _cartRepository.SaveAsync();

        return "با موفقیت به سبد خرید اضافه شد";
    }

    public async Task UpdateItemQuantityAsync(UpdateCartDto dto)
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");
        if (dto.NewQuantity <= 0)
            throw new Exception("تعداد باید بیشتر از صفر باشد. برای حذف، از متد حذف استفاده کنید.");

        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart is null) throw new Exception("سبد خرید یافت نشد.");

        var item = cart.CartItems.FirstOrDefault(x => x.ProductId == dto.ProductId);
        if (item is null) throw new Exception("این محصول در سبد خرید شما وجود ندارد.");

        // Todo : Warehousing repo for check stock
        var product = await _productRepository.GetProductByIdAsync(dto.ProductId);
        if (product is null) throw new Exception("محصول یافت نشد.");
        if (product.Stock < dto.NewQuantity)
            throw new Exception($"موجودی انبار کافی نیست. حداکثر موجودی: {product.Stock}");

        item.Quantity = dto.NewQuantity;
        // Todo: Update Timespan
        // cart.UpdateTimestamp();

        await _cartRepository.SaveAsync();
    }

    public async Task<ViewCartDto> GetCartByUserIdAsync()
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");
        var cart = await _cartRepository.GetCartWithProductsByUserIdAsync(userId);
        if (cart is null)
        {
            return new ViewCartDto
            {
                UserId = userId,
                Items = new List<ViewCartItemsDto>()
            };
        }

        var cartDto = new ViewCartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.CartItems.Select(x => new ViewCartItemsDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductTitle = x.Product.Title,
                Quantity = x.Quantity,
                UnitPrice = x.Product.Price
            }).ToList()
        };
        return cartDto;
    }

    public async Task DeleteItemAsync(Guid productId)
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart is null) throw new Exception("سبد خرید یافت نشد");

        var item = cart.CartItems.FirstOrDefault(x => x.ProductId == productId);
        if (item is null) throw new Exception("محصول در سبد خرید یافت نشد");

        cart.CartItems.Remove(item);
        // Todo: Update Timespan
        // cart.UpdateTimestamp();
        await _cartRepository.SaveAsync();
    }

    public async Task ClearCartAsync()
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart is null) throw new Exception("سبد خرید یافت نشد");

        cart.ClearCart();
        await _cartRepository.SaveAsync();
    }

    public async Task<int> GetCartItemsCountAsync()
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart is null) return 0;

        return cart.CartItems.Sum(x => x.Quantity);
    }
}