using Application.Features.Inventory.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class InventoryRepository : InventoryRepositoryContract
{
    private readonly ShopDbContext _context;

    public InventoryRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryItem?> GetByProductVariantIdAsync(Guid productVariantId)
    {
        return await _context
            .InventoryItems
            .Include(it => it.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .Include(it => it.Transactions.OrderByDescending(t => t.CreatedAt))
            .Include(it => it.ProductVariant)
            .ThenInclude(pv => pv.Options)
            .ThenInclude(pvo => pvo.ProductOption)
            .Include(it => it.ProductVariant)
            .ThenInclude(pv => pv.Options)
            .ThenInclude(pvo => pvo.ProductOptionValue)
            .FirstOrDefaultAsync(it => it.ProductVariantId == productVariantId);
    }

    public async Task<List<InventoryItem>?> GetByProductIdsAsync(List<Guid> productVariantIds)
    {
        return await _context
            .InventoryItems
            .Include(it => it.ProductVariant)
            .Where(it => productVariantIds.Contains(it.ProductVariantId))
            .ToListAsync();
    }

    public async Task<InventoryItem?> GetByIdAsync(Guid id)
    {
        return await _context
            .InventoryItems
            .FirstOrDefaultAsync(it => it.InventoryId == id);
    }

    public async Task<List<InventoryItem>?> GetAllAsync()
    {
        return await _context
            .InventoryItems
            .Include(it => it.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .Include(it => it.Transactions.OrderByDescending(t => t.CreatedAt))
            .Include(it => it.ProductVariant)
            .ThenInclude(pv => pv.Options)
            .ThenInclude(pvo => pvo.ProductOption)
            .Include(it => it.ProductVariant)
            .ThenInclude(pv => pv.Options)
            .ThenInclude(pvo => pvo.ProductOptionValue)
            .ToListAsync();
    }

    public void UpdateAsync(InventoryItem inventoryItem)
    {
        _context.Update(inventoryItem);
    }

    public async Task AddAsync(InventoryItem inventory)
    {
        await _context.InventoryItems.AddAsync(inventory);
    }

    public async Task<InventoryItem?> GetByProductVariantId(Guid productVariantId)
    {
        return await _context
            .InventoryItems
            .Include(it => it.ProductVariant)
            .Include(it => it.Transactions.OrderByDescending(t => t.CreatedAt).Take(10))
            .FirstOrDefaultAsync(it => it.ProductVariantId == productVariantId);
    }

    public async Task<List<InventoryItem>?> GetByProductVariantIdsAsync(List<Guid> productVariantIds)
    {
        return await _context
            .InventoryItems
            .Include(it => it.ProductVariant)
            .Where(it => productVariantIds.Contains(it.ProductVariantId))
            .ToListAsync();
    }
}