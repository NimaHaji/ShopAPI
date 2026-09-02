using System.Diagnostics;
using Application.Common.Interfaces;
using Application.Common.Observability;
using Application.Features.Product.Interfaces;
using Application.Features.Wishlist.DTOs;
using Application.Features.Wishlist.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Exceptions;

namespace Application.Features.Wishlist.Implementations;

public class WishlistService : WishlistServiceContract
{
    private readonly WishlistRepositoryContract _wishlistRepositoryContract;
    private readonly IUSerContext _userContext;
    private readonly ProductRepositoryContract _productRepositoryContract;
    private readonly WishlistItemRepositoryContract _wishlistItemRepositoryContract;
    private readonly UnitOfWorkContract _unitOfWorkContract;
    private readonly ILogger<WishlistService> _logger;

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
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(GetWishlistAsync));

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است .");

        activity?.SetTag("user.id", userId);

        var wishlist =
            await _wishlistRepositoryContract
                .GetWishlistByUserId(userId);

        if (wishlist is null)
        {
            activity?.SetTag("wishlist.items_count", 0);

            return new ViewWishlistDto
            {
                Id = null,
                WishlistItems = []
            };
        }

        var items = wishlist.WishlistItems
            .Select(wi => new ViewWishlistItemDto
            {
                Id = wi.Id,
                Title = wi.Product.Title,
                ProductId = wi.ProductId,
                ImageUrl = wi.Product.Images
                    .Select(i => i.ImageLink)
                    .FirstOrDefault(),
                AddedAt = wi.AddedAt,
                Price = wi.Product.Variants
                    .Select(pv => pv.Price)
                    .Min()
            })
            .ToList();

        activity?.SetTag(
            "wishlist.id",
            wishlist.Id);

        activity?.SetTag(
            "wishlist.items_count",
            items.Count);

        return new ViewWishlistDto
        {
            Id = wishlist.Id,
            WishlistItems = items
        };
    }

    public async Task<string> AddProductToWishlistAsync(
        AddWishlistItemDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(AddProductToWishlistAsync));

        if (dto.ProductId == Guid.Empty)
        {
            _logger.LogWarning(
                LogEvents.Wishlist.InvalidProduct,
                "Wishlist add rejected because product id is invalid.");

            throw new BusinessException(
                "شناسه محصول نامعتبر است.");
        }

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);
        activity?.SetTag("product.id", dto.ProductId);

        _logger.LogInformation(
            LogEvents.Wishlist.AddStarted,
            "Adding product to wishlist started. ProductId: {ProductId}",
            dto.ProductId);

        var product =
            await _productRepositoryContract
                .GetProductByIdAsync(dto.ProductId);

        if (product is null)
        {
            _logger.LogWarning(
                LogEvents.Wishlist.ProductNotFound,
                "Wishlist add rejected because product was not found. ProductId: {ProductId}",
                dto.ProductId);

            throw new NotFoundException(
                "محصول یافت نشد.");
        }

        var wishlist =
            await _wishlistRepositoryContract
                .GetWishlistByUserId(userId);

        if (wishlist is null)
        {
            wishlist =
                new Domain.Entities.Wishlist(userId);

            await _wishlistRepositoryContract
                .AddWishlistAsync(wishlist);

            await _unitOfWorkContract.SaveAsync();
        }

        activity?.SetTag(
            "wishlist.id",
            wishlist.Id);

        var exists =
            await _wishlistItemRepositoryContract
                .ExistsAsync(
                    wishlist.Id,
                    dto.ProductId);

        if (exists)
        {
            _logger.LogWarning(
                LogEvents.Wishlist.Duplicate,
                "Product was already in wishlist. ProductId: {ProductId}, WishlistId: {WishlistId}",
                dto.ProductId,
                wishlist.Id);

            throw new BusinessException(
                "این محصول قبلاً به علاقه‌مندی‌ها اضافه شده است.");
        }

        var wishlistItem = new WishlistItem(
            productId: dto.ProductId,
            wishlistId: wishlist.Id);

        await _wishlistItemRepositoryContract
            .AddWishlistItem(wishlistItem);

        await _unitOfWorkContract.SaveAsync();

        activity?.SetTag(
            "wishlist_item.id",
            wishlistItem.Id);

        activity?.SetStatus(
            ActivityStatusCode.Ok);

        _logger.LogInformation(
            LogEvents.Wishlist.AddCompleted,
            "Product added to wishlist successfully. ProductId: {ProductId}, WishlistId: {WishlistId}",
            dto.ProductId,
            wishlist.Id);

        return "محصول با موفقیت به علاقه‌مندی‌ها اضافه شد.";
    }

    public async Task<string> DeleteProductFromWishListAsync(
        Guid productId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(DeleteProductFromWishListAsync));

        if (productId == Guid.Empty)
        {
            _logger.LogWarning(
                LogEvents.Wishlist.InvalidProduct,
                "Wishlist delete rejected because product id is invalid.");

            throw new BusinessException(
                "شناسه محصول نامعتبر است.");
        }

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);
        activity?.SetTag("product.id", productId);

        _logger.LogInformation(
            LogEvents.Wishlist.DeleteStarted,
            "Removing product from wishlist started. ProductId: {ProductId}",
            productId);

        var product =
            await _productRepositoryContract
                .GetProductByIdAsync(productId);

        if (product is null)
        {
            _logger.LogWarning(
                LogEvents.Wishlist.ProductNotFound,
                "Wishlist delete rejected because product was not found. ProductId: {ProductId}",
                productId);

            throw new NotFoundException(
                "محصول یافت نشد.");
        }

        var wishlist =
            await _wishlistRepositoryContract
                .GetWishlistByUserId(userId);

        if (wishlist is null)
        {
            _logger.LogWarning(
                LogEvents.Wishlist.NotFound,
                "Wishlist was not found. UserId: {UserId}",
                userId);

            throw new NotFoundException(
                "علاقه‌مندی یافت نشد.");
        }

        activity?.SetTag(
            "wishlist.id",
            wishlist.Id);

        var wishlistItem =
            await _wishlistItemRepositoryContract
                .GetWishlistItemAsync(
                    wishlist.Id,
                    productId);

        if (wishlistItem is null)
        {
            _logger.LogWarning(
                LogEvents.Wishlist.NotFound,
                "Product was not found in wishlist. ProductId: {ProductId}, WishlistId: {WishlistId}",
                productId,
                wishlist.Id);

            throw new NotFoundException(
                "این محصول در علاقه‌مندی‌های شما وجود ندارد.");
        }

        await _wishlistItemRepositoryContract
            .DeleteWishlistItem(wishlistItem);

        await _unitOfWorkContract.SaveAsync();

        activity?.SetStatus(
            ActivityStatusCode.Ok);

        _logger.LogInformation(
            LogEvents.Wishlist.DeleteCompleted,
            "Product removed from wishlist successfully. ProductId: {ProductId}, WishlistId: {WishlistId}",
            productId,
            wishlist.Id);

        return "محصول با موفقیت از علاقه‌مندی‌ها حذف شد.";
    }

    public async Task<string> ClearWishListAsync()
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(ClearWishListAsync));

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);

        _logger.LogInformation(
            LogEvents.Wishlist.ClearStarted,
            "Clearing wishlist started. UserId: {UserId}",
            userId);

        var wishlist =
            await _wishlistRepositoryContract
                .GetWishlistByUserId(userId);

        if (wishlist is null)
        {
            _logger.LogWarning(
                LogEvents.Wishlist.NotFound,
                "Wishlist clear rejected because wishlist was not found. UserId: {UserId}",
                userId);

            throw new NotFoundException(
                "علاقه‌مندی یافت نشد.");
        }

        activity?.SetTag(
            "wishlist.id",
            wishlist.Id);

        await _wishlistItemRepositoryContract
            .ClearWishlistAsync(wishlist.Id);

        await _unitOfWorkContract.SaveAsync();

        activity?.SetStatus(
            ActivityStatusCode.Ok);

        _logger.LogInformation(
            LogEvents.Wishlist.ClearCompleted,
            "Wishlist cleared successfully. WishlistId: {WishlistId}",
            wishlist.Id);

        return "لیست علاقه مندی با موفقیت خالی شد .";
    }

    public async Task<int> GetWishlistItemsCountAsync()
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(GetWishlistItemsCountAsync));

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است .");

        activity?.SetTag("user.id", userId);

        var wishlist =
            await _wishlistRepositoryContract
                .GetWishlistByUserId(userId);

        if (wishlist is null)
        {
            activity?.SetTag(
                "wishlist.items_count",
                0);

            return 0;
        }

        activity?.SetTag(
            "wishlist.id",
            wishlist.Id);

        activity?.SetTag(
            "wishlist.items_count",
            wishlist.WishlistItems.Count);

        return wishlist.WishlistItems.Count;
    }
}