using Application.Common.Interfaces;
using Application.Common.Observability;
using Application.Features.Inventory.DTOs;
using Application.Features.Inventory.Interfaces;
using Application.Features.InventoryTransaction.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Exceptions;

namespace Application.Features.Inventory.Implement;

public class InventoryService : InventoryServiceContract
{
    private readonly InventoryRepositoryContract _inventoryRepositoryContract;
    private readonly UnitOfWorkContract _unitOfWorkContract;
    private readonly InventoryTransactionRepositoryContract _repositoryTransactionContract;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(
        InventoryRepositoryContract inventoryRepositoryContract,
        UnitOfWorkContract unitOfWorkContract, InventoryTransactionRepositoryContract repositoryTransactionContract, ILogger<InventoryService> logger)
    {
        _inventoryRepositoryContract = inventoryRepositoryContract;
        _unitOfWorkContract = unitOfWorkContract;
        _repositoryTransactionContract = repositoryTransactionContract;
        _logger = logger;
    }

    public async Task<ViewInventoryItemDto> GetInventoryByProductVariantIdAsync(
        Guid productVariantId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(GetInventoryByProductVariantIdAsync));

        activity?.SetTag("product_variant.id", productVariantId);

        var inventory =
            await _inventoryRepositoryContract
                .GetByProductVariantIdAsync(productVariantId);

        if (inventory is null)
        {
            _logger.LogWarning(
                LogEvents.Inventory.InsufficientStock,
                "Inventory not found. ProductVariantId: {ProductVariantId}",
                productVariantId);

            throw new NotFoundException(
                $"موجودی برای {productVariantId} پیدا نشد");
        }

        return MapToDto(inventory);
    }

    public async Task<ViewInventoryItemDto> ReserveStockAsync(
        Guid productVariantId,
        int quantity,
        string orderReference)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(ReserveStockAsync));

        activity?.SetTag("product_variant.id", productVariantId);
        activity?.SetTag("inventory.quantity", quantity);
        activity?.SetTag("order.reference", orderReference);

        const int maxRetries = 3;

        for (int retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                var inventory =
                    await _inventoryRepositoryContract
                        .GetByProductVariantId(productVariantId);

                if (inventory is null)
                    throw new NotFoundException(
                        $"موجودی برای {productVariantId} پیدا نشد");

                var availableQuantity =
                    inventory.StockQuantity -
                    inventory.ReservedQuantity;

                if (availableQuantity < quantity)
                {
                    _logger.LogWarning(
                        LogEvents.Inventory.InsufficientStock,
                        "Stock reservation rejected due to insufficient stock. ProductVariantId: {ProductVariantId}, Requested: {Requested}, Available: {Available}",
                        productVariantId,
                        quantity,
                        availableQuantity);

                    throw new InsufficientStockException(
                        $"موجودی ناکافی برای محصول {productVariantId}");
                }

                var transaction =
                    new Domain.Entities.InventoryTransaction(
                        inventoryItemId: inventory.InventoryId,
                        transactionType: TransactionType.Reservation,
                        quantity: quantity,
                        reference: orderReference,
                        description:
                        $"محصول {productVariantId} با شماره سفارش {orderReference} رزرو شد"
                    );

                inventory.Reserve(quantity);
                inventory.Transactions.Add(transaction);

                await _repositoryTransactionContract
                    .AddInventoryTransactionAsync(transaction);

                await _unitOfWorkContract.SaveAsync();

                activity?.SetTag(
                    "inventory.available_quantity",
                    availableQuantity - quantity);

                _logger.LogInformation(
                    LogEvents.Inventory.StockReserved,
                    "Stock reserved successfully. ProductVariantId: {ProductVariantId}, Quantity: {Quantity}, OrderReference: {OrderReference}",
                    productVariantId,
                    quantity,
                    orderReference);

                return MapToDto(inventory);
            }
            catch (DbUpdateConcurrencyException)
            {
                _unitOfWorkContract.ClearChangeTracker();

                _logger.LogWarning(
                    LogEvents.Inventory.ConcurrencyConflict,
                    "Inventory concurrency conflict. ProductVariantId: {ProductVariantId}, Attempt: {Attempt}, MaxAttempts: {MaxAttempts}",
                    productVariantId,
                    retry + 1,
                    maxRetries);

                if (retry == maxRetries - 1)
                {
                    throw new ConflictException(
                        "موجودی تغییر کرده است صفحه را refresh کنید");
                }

                await Task.Delay(50 * (retry + 1));
            }
        }

        throw new InvalidOperationException(
            "خطای غیرمنتظره در رزرو موجودی");
    }

    public async Task ReserveAllItemStockAsync(
        List<Domain.Entities.CartItem> items)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(ReserveAllItemStockAsync));

        activity?.SetTag("checkout.items_count", items.Count);

        var productVariantIds =
            items
                .Select(i => i.ProductVariantId)
                .Distinct()
                .ToList();

        activity?.SetTag(
            "checkout.unique_variants_count",
            productVariantIds.Count);

        var inventories =
            await _inventoryRepositoryContract
                .GetByProductVariantIdsAsync(productVariantIds);

        foreach (var item in items)
        {
            var inventory =
                inventories?.SingleOrDefault(x => x.ProductVariantId == item.ProductVariantId);

            if (inventory is null)
                throw new NotFoundException(
                    "موجودی محصول پیدا نشد");

            var availableQuantity =
                inventory.StockQuantity -
                inventory.ReservedQuantity;

            if (availableQuantity < item.Quantity)
            {
                _logger.LogWarning(
                    LogEvents.Inventory.InsufficientStock,
                    "Stock reservation rejected due to insufficient stock. ProductVariantId: {ProductVariantId}, Requested: {Requested}, Available: {Available}",
                    item.ProductVariantId,
                    item.Quantity,
                    availableQuantity);

                throw new InsufficientStockException(
                    $"موجودی ناکافی برای محصول {item.ProductVariantId}");
            }

            var transaction =
                new Domain.Entities.InventoryTransaction(
                    inventoryItemId: inventory.InventoryId,
                    transactionType: TransactionType.Reservation,
                    quantity: item.Quantity,
                    reference: "رزرو",
                    description:
                    $"محصول {item.ProductVariantId} رزرو شد"
                );

            inventory.Reserve(item.Quantity);

            await _repositoryTransactionContract
                .AddInventoryTransactionAsync(transaction);
        }

        _logger.LogInformation(
            LogEvents.Inventory.StockReserved,
            "Stock reserved successfully for cart items. ItemCount: {ItemCount}",
            items.Count);
    }

    public async Task<ViewInventoryItemDto> ConfirmReservationAsync(
        Guid productVariantId,
        int quantity,
        string orderReference)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(ConfirmReservationAsync));

        activity?.SetTag("product_variant.id", productVariantId);
        activity?.SetTag("inventory.quantity", quantity);
        activity?.SetTag("order.reference", orderReference);

        var inventory =
            await _inventoryRepositoryContract
                .GetByProductVariantId(productVariantId);

        if (inventory is null)
            throw new NotFoundException(
                $"محصول {productVariantId} یافت نشد");

        if (inventory.ReservedQuantity < quantity)
            throw new InvalidOperationException(
                $"تعداد درخواستی برای تأیید از تعداد رزروشده بیشتر است. ({quantity} > {inventory.ReservedQuantity})");

        var transaction =
            new Domain.Entities.InventoryTransaction(
                inventoryItemId: inventory.InventoryId,
                transactionType: TransactionType.Confirmation,
                quantity: quantity,
                reference: orderReference,
                description:
                $"محصول {productVariantId} با شماره سفارش {orderReference} قبول شد"
            );

        inventory.CommitReserve(quantity);
        inventory.Transactions.Add(transaction);

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Inventory.StockReserved,
            "Stock reservation confirmed. ProductVariantId: {ProductVariantId}, Quantity: {Quantity}, OrderReference: {OrderReference}",
            productVariantId,
            quantity,
            orderReference);

        return MapToDto(inventory);
    }

    public async Task<ViewInventoryItemDto> CancelReservationAsync(
        Guid productVariantId,
        int quantity,
        string orderReference)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(CancelReservationAsync));

        activity?.SetTag("product_variant.id", productVariantId);
        activity?.SetTag("inventory.quantity", quantity);
        activity?.SetTag("order.reference", orderReference);

        var inventory =
            await _inventoryRepositoryContract
                .GetByProductVariantId(productVariantId);

        if (inventory is null)
            throw new NotFoundException(
                $"محصول {productVariantId} یافت نشد");

        if (inventory.ReservedQuantity < quantity)
            throw new InvalidOperationException(
                $"تعداد درخواستی برای لغو از تعداد رزروشده بیشتر است. ({quantity} > {inventory.ReservedQuantity})");

        var transaction =
            new Domain.Entities.InventoryTransaction(
                inventoryItemId: inventory.InventoryId,
                transactionType: TransactionType.Cancellation,
                quantity: quantity,
                reference: orderReference,
                description:
                $"محصول {productVariantId} با شماره سفارش {orderReference} کنسل شد"
            );

        inventory.CancelReserve(quantity);
        inventory.Transactions.Add(transaction);

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Inventory.StockReserved,
            "Stock reservation cancelled. ProductVariantId: {ProductVariantId}, Quantity: {Quantity}, OrderReference: {OrderReference}",
            productVariantId,
            quantity,
            orderReference);

        return MapToDto(inventory);
    }

    public async Task<ViewInventoryItemDto> AddStockAsync(
        Guid productVariantId,
        int quantity,
        string description)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(
                nameof(AddStockAsync));

        activity?.SetTag("product_variant.id", productVariantId);
        activity?.SetTag("inventory.quantity", quantity);

        var inventory =
            await _inventoryRepositoryContract
                .GetByProductVariantId(productVariantId);

        if (inventory is null)
        {
            inventory = new InventoryItem(
                productVariantId: productVariantId,
                stockQuantity: 0,
                reservedQuantity: 0
            );

            await _inventoryRepositoryContract.AddAsync(inventory);
        }

        var transaction =
            new Domain.Entities.InventoryTransaction(
                inventoryItemId: inventory.InventoryId,
                transactionType: TransactionType.StockIn,
                quantity: quantity,
                reference: "افزایش محصول",
                description: description
            );

        inventory.AddStockQuantity(quantity);
        inventory.Transactions.Add(transaction);

        _logger.LogInformation(
            LogEvents.Inventory.StockReserved,
            "Stock added successfully. ProductVariantId: {ProductVariantId}, Quantity: {Quantity}",
            productVariantId,
            quantity);

        return MapToDto(inventory);
    }

    public async Task<List<ViewInventoryItemDto>> GetAllInventoryAsync()
    {
        var items = await _inventoryRepositoryContract.GetAllAsync();
        return items?.Select(MapToDto).ToList() ?? new List<ViewInventoryItemDto>();
    }

    private ViewInventoryItemDto MapToDto(InventoryItem item)
    {
        var variant = item.ProductVariant;

        return new ViewInventoryItemDto
        {
            InventoryId = item.InventoryId,

            ProductVariantId = item.ProductVariantId,

            ProductId = variant.ProductId,

            ProductTitle = variant.Product.Title,

            VariantSku = variant.Sku,

            Price = variant.Price,

            StockQuantity = item.StockQuantity,

            ReservedQuantity = item.ReservedQuantity,

            AvailableQuantity = item.AvailableQuantity,

            LastUpdated = item.LastUpdated,

            Options = variant.Options
                .Select(option => new ViewInventoryVariantOptionDto
                {
                    Name = option.ProductOption.Name,

                    Value = option.ProductOptionValue.Value
                })
                .ToList(),

            RecentTransactions = item.Transactions
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .Select(t => new ViewTransactionDto
                {
                    Id = t.InventoryTransactionId,
                    Type = t.Type.ToString(),
                    Quantity = t.Quantity,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt
                })
                .ToList()
        };
    }
}