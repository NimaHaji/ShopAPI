using Application.Common.Interfaces;
using Application.Features.Product.Interfaces;
using Application.Features.Wishlist.DTOs;
using Application.Features.Wishlist.Interfaces;
using Domain.Entities;
using Shared.Exceptions;

namespace Application.Features.Wishlist.Implementations;

public class WishlistService : WishlistServiceContract
{
    private readonly WishlistRepositoryContract _wishlistRepositoryContract;
    private readonly IUSerContext _userContext;
    private readonly ProductRepositoryContract _productRepositoryContract;
    private readonly WishlistItemRepositoryContract _wishlistItemRepositoryContract;
    private readonly UnitOfWorkContract _unitOfWorkContract;

    public WishlistService(WishlistRepositoryContract wishlistRepositoryContract, IUSerContext userContext,
        WishlistItemRepositoryContract wishlistItemRepositoryContract, UnitOfWorkContract unitOfWorkContract,
        ProductRepositoryContract productRepositoryContract)
    {
        _wishlistRepositoryContract = wishlistRepositoryContract;
        _userContext = userContext;
        _wishlistItemRepositoryContract = wishlistItemRepositoryContract;
        _unitOfWorkContract = unitOfWorkContract;
        _productRepositoryContract = productRepositoryContract;
    }

    public async Task<ViewWishlistDto> GetWishlistAsync()
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است .");

        var wishlist = await _wishlistRepositoryContract.GetWishlistByUserId(userId);

        if (wishlist == null)
            return new ViewWishlistDto
            {
                Id = null,
                WishlistItems = []
            };

        var dto = new ViewWishlistDto
        {
            Id = wishlist.Id,
            WishlistItems = wishlist.WishlistItems.Select(wi => new ViewWishlistItemDto
            {
                Id = wi.Id,
                Title = wi.Product.Title,
                ProductId = wi.ProductId,
                ImageUrl = wi.Product.Images
                    .Select(i => i.ImageLink)
                    .FirstOrDefault(),
                AddedAt = wi.AddedAt,
                Price = wi.Product.Variants.Select(pv => pv.Price).Min()
            }).ToList()
        };
        return dto;
    }

    public async Task<string> AddProductToWishlistAsync(AddWishlistItemDto dto)
    {
        if (dto.ProductId == Guid.Empty)
            throw new BusinessException("شناسه محصول نامعتبر است.");

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");

        var product = await _productRepositoryContract.GetProductByIdAsync(dto.ProductId);

        if (product is null)
            throw new NotFoundException("محصول یافت نشد.");

        var wishlist = await _wishlistRepositoryContract.GetWishlistByUserId(userId);

        if (wishlist is null)
        {
            wishlist = new Domain.Entities.Wishlist(userId);

            await _wishlistRepositoryContract.AddWishlistAsync(wishlist);
            await _unitOfWorkContract.SaveAsync();
        }

        var exists = await _wishlistItemRepositoryContract
            .ExistsAsync(wishlist.Id, dto.ProductId);

        if (exists)
            throw new BusinessException("این محصول قبلاً به علاقه‌مندی‌ها اضافه شده است.");

        var wishlistItem = new WishlistItem(
            productId: dto.ProductId,
            wishlistId: wishlist.Id
        );

        await _wishlistItemRepositoryContract.AddWishlistItem(wishlistItem);

        await _unitOfWorkContract.SaveAsync();

        return "محصول با موفقیت به علاقه‌مندی‌ها اضافه شد.";
    }

    public async Task<string> DeleteProductFromWishListAsync(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new BusinessException("شناسه محصول نامعتبر است.");

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");

        var product = await _productRepositoryContract.GetProductByIdAsync(productId);

        if (product is null)
            throw new NotFoundException("محصول یافت نشد.");

        var wishlist = await _wishlistRepositoryContract.GetWishlistByUserId(userId);

        if (wishlist is null)
            throw new NotFoundException("علاقه‌مندی یافت نشد.");

        var wishlistItem = await _wishlistItemRepositoryContract
            .GetWishlistItemAsync(wishlist.Id, productId);

        if (wishlistItem is null)
            throw new NotFoundException("این محصول در علاقه‌مندی‌های شما وجود ندارد.");

        await _wishlistItemRepositoryContract.DeleteWishlistItem(wishlistItem);

        await _unitOfWorkContract.SaveAsync();

        return "محصول با موفقیت از علاقه‌مندی‌ها حذف شد.";
    }

    public async Task<string> ClearWishListAsync()
    {
        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");

        var wishlist = await _wishlistRepositoryContract.GetWishlistByUserId(userId);

        if (wishlist is null)
            throw new NotFoundException("علاقه‌مندی یافت نشد.");

        await _wishlistItemRepositoryContract.ClearWishlistAsync(wishlist.Id);

        await _unitOfWorkContract.SaveAsync();

        return "لیست علاقه مندی با موفقیت خالی شد .";
    }

    public async Task<int> GetWishlistItemsCountAsync()
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است .");

        var wishlist = await _wishlistRepositoryContract.GetWishlistByUserId(userId);

        if (wishlist == null)
            return 0;

        return wishlist.WishlistItems.Count;
    }
}