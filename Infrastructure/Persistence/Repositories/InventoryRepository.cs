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

    public async Task<InventoryItem?> GetByProductIdAsync(Guid productId)
    {
        return await _context
            .InventoryItems
            .Include(it => it.Product)
            .Include(it => it.Transactions.OrderByDescending(t => t.CreatedAt).Take(10))
            .FirstOrDefaultAsync(it => it.ProductId == productId);
    }

    public async Task<List<InventoryItem>?> GetByProductIdsAsync(List<Guid> productId)
    {
        return await _context
            .InventoryItems
            .Include(it => it.Product)
            .Where(it => productId.Contains(it.ProductId))
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
            .Include(it => it.Product)
            .ToListAsync();
    }

    public void UpdateAsync(InventoryItem  inventoryItem)
    {
         _context.Update(inventoryItem);
    }

    public async Task AddAsync(InventoryItem inventory)
    {
        await _context.InventoryItems.AddAsync(inventory);
    }

    public async Task<InventoryItem?> GetByProductIdWithLockAsync(Guid productId)
    {
        return await _context
            .InventoryItems
            .Include(it => it.Product)
            .Include(it => it.Transactions.OrderByDescending(t => t.CreatedAt).Take(10))
            .FirstOrDefaultAsync(it => it.ProductId == productId);
    }
}