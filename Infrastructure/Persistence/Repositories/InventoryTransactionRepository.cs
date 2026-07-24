using Application.Features.InventoryTransaction.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;

namespace Infrastructure.Persistence.Repositories;

public class InventoryTransactionRepository:InventoryTransactionRepositoryContract
{
    private readonly ShopDbContext _context;

    public InventoryTransactionRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task AddInventoryTransactionAsync(InventoryTransaction inventoryTransaction)
    {
        await _context.InventoryTransactions.AddAsync(inventoryTransaction);
    }
}