using System.Data;
using Application.Common.Interfaces;
using Application.Features.Inventory.DTOs;
using Application.Features.Inventory.Interfaces;
using Application.Features.InventoryTransaction.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;

namespace Application.Features.Inventory.Implement;

public class InventoryService : InventoryServiceContract
{
    private readonly InventoryRepositoryContract _inventoryRepositoryContract;
    private readonly InventoryTransactionRepositoryContract _inventoryTransactionRepositoryContract;
    private readonly UnitOfWorkContract _unitOfWorkContract;
    private readonly InventoryTransactionRepositoryContract _repositoryTransactionContract;

    public InventoryService(
        InventoryRepositoryContract inventoryRepositoryContract,
        UnitOfWorkContract unitOfWorkContract, InventoryTransactionRepositoryContract repositoryTransactionContract,
        InventoryTransactionRepositoryContract inventoryTransactionRepositoryContract)
    {
        _inventoryRepositoryContract = inventoryRepositoryContract;
        _unitOfWorkContract = unitOfWorkContract;
        _repositoryTransactionContract = repositoryTransactionContract;
        _inventoryTransactionRepositoryContract = inventoryTransactionRepositoryContract;
    }

    public async Task<InventoryItemDto> GetInventoryByProductIdAsync(Guid productId)
    {
        var inventory = await _inventoryRepositoryContract.GetByProductIdAsync(productId);

        if (inventory == null)
            throw new NotFoundException($"موجودی برای {productId}پیدا نشد");

        return MapToDto(inventory);
    }

    public async Task<InventoryItemDto> ReserveStockAsync(Guid productId, int quantity, string orderReference)
    {
        // Todo : Concurrency fix 
        const int maxRetries = 3;

        for (int retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                var inventory = await _inventoryRepositoryContract.GetByProductId(productId);

                if (inventory == null)
                    throw new NotFoundException($"موجودی برای {productId} پیدا نشد");

                if (inventory.StockQuantity - inventory.ReservedQuantity < quantity)
                    throw new InsufficientStockException($"موجودی ناکافی برای محصول {productId}");

                var transaction = new Domain.Entities.InventoryTransaction(
                    inventoryItemId: inventory.InventoryId,
                    transactionType: TransactionType.Reservation,
                    quantity: quantity,
                    reference: orderReference,
                    description: $"محصول {productId} با شماره سفارش {orderReference} رزرو شد"
                );

                inventory.Reserve(quantity);
                inventory.Transactions.Add(transaction);

                _repositoryTransactionContract.AddInventoryTransactionAsync(transaction);

                await _unitOfWorkContract.SaveAsync();

                return MapToDto(inventory);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _unitOfWorkContract.ClearChangeTracker();
                if (retry == maxRetries - 1)
                    throw new ConflictException("موجودی تغییر کرده است صفحه را refresh کنید");
                await Task.Delay(50 * (retry + 1));
            }
        }

        throw new InvalidOperationException("خطای غیرمنتظره در رزرو موجودی");
    }

    public async Task ReserveAllItemStockAsync(List<CartItem> items)
    {
        var productIds = items.Select(i => i.ProductId).Distinct().ToList();
        var inventories = await _inventoryRepositoryContract.GetByProductIdsAsync(productIds);

        foreach (var item in items)
        {
            var inventory = inventories?.SingleOrDefault(x => x.ProductId == item.ProductId);
            if (inventory is null)
                throw new NotFoundException("موجودی محصوب پیدا نشد");

            var transaction = new Domain.Entities.InventoryTransaction(
                inventoryItemId: inventory.InventoryId,
                transactionType: TransactionType.Reservation,
                quantity: item.Quantity,
                reference: "رزرو",
                description: $"محصول {item.ProductId} رزرو شد"
            );

            inventory.Reserve(item.Quantity);
            await _repositoryTransactionContract.AddInventoryTransactionAsync(transaction);
        }
    }

    public async Task<InventoryItemDto> ConfirmReservationAsync(Guid productId, int quantity, string orderReference)
    {
        var inventory = await _inventoryRepositoryContract.GetByProductId(productId);

        if (inventory == null)
            throw new NotFoundException($"محصول {productId} یافت نشد ");

        if (inventory.ReservedQuantity < quantity)
            throw new InvalidOperationException(
                $"تعداد درخواستی برای تأیید از تعداد رزروشده بیشتر است. ({quantity} > {inventory.ReservedQuantity})");

        var transaction = new Domain.Entities.InventoryTransaction(
            inventoryItemId: inventory.InventoryId,
            transactionType: TransactionType.Confirmation,
            quantity: quantity,
            reference: orderReference,
            description: $"محصول {productId} با شماره سفارش {orderReference} قبول شد"
        );

        inventory.CommitReserve(quantity);
        inventory.Transactions.Add(transaction);

        await _unitOfWorkContract.SaveAsync();

        return MapToDto(inventory);
    }

    public async Task<InventoryItemDto> CancelReservationAsync(Guid productId, int quantity, string orderReference)
    {
        var inventory = await _inventoryRepositoryContract.GetByProductId(productId);

        if (inventory == null)
            throw new NotFoundException($"محصول {productId} یافت نشد ");

        if (inventory.ReservedQuantity < quantity)
            throw new InvalidOperationException(
                $"تعداد درخواستی برای تأیید از تعداد رزروشده بیشتر است. ({quantity} > {inventory.ReservedQuantity})");

        var transaction = new Domain.Entities.InventoryTransaction(
            inventoryItemId: inventory.InventoryId,
            transactionType: TransactionType.Cancellation,
            quantity: quantity,
            reference: orderReference,
            description: $"محصول {productId} با شماره سفارش {orderReference} کنسل شد"
        );

        inventory.CancelReserve(quantity);
        inventory.Transactions.Add(transaction);

        await _unitOfWorkContract.SaveAsync();

        return MapToDto(inventory);
    }

    public async Task<InventoryItemDto> AddStockAsync(Guid productId, int quantity, string description)
    {
        var inventory = await _inventoryRepositoryContract.GetByProductId(productId);

        if (inventory == null)
        {
            inventory = new InventoryItem(
                productId: productId,
                stockQuantity: 0,
                reservedQuantity: 0
            );
            await _inventoryRepositoryContract.AddAsync(inventory);
        }

        var transaction = new Domain.Entities.InventoryTransaction(
            inventoryItemId: inventory.InventoryId,
            transactionType: TransactionType.StockIn,
            quantity: quantity,
            reference: "افزایش محصول",
            description: description
        );

        inventory.AddStockQuantity(quantity);
        inventory.Transactions.Add(transaction);

        return MapToDto(inventory);
    }

    public async Task<List<InventoryItemDto>> GetAllInventoryAsync()
    {
        var items = await _inventoryRepositoryContract.GetAllAsync();
        return items?.Select(MapToDto).ToList() ?? new List<InventoryItemDto>();
    }

    private InventoryItemDto MapToDto(InventoryItem item)
    {
        return new InventoryItemDto
        {
            InventoryId = item.InventoryId,
            ProductId = item.ProductId,
            ProductName = item.Product?.Title,
            StockQuantity = item.StockQuantity,
            ReservedQuantity = item.ReservedQuantity,
            AvailableQuantity = item.StockQuantity - item.ReservedQuantity,
            LastUpdated = item.LastUpdated,
            RecentTransactions = item.Transactions?.Take(10).Select(t => new TransactionDto
            {
                Id = t.InventoryTransactionId,
                Type = t.Type.ToString(),
                Quantity = t.Quantity,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            }).ToList()
        };
    }
}