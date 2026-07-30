using System.Data;
using Application.Common.Interfaces;
using Application.Features.Cart.Interfaces;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistence;

public class UnitOfWork : UnitOfWorkContract
{
    private readonly ShopDbContext _shopDbContext;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ShopDbContext shopDbContext)
    {
        _shopDbContext = shopDbContext;
    }

    public async Task<int> SaveAsync()
    {
        return await _shopDbContext.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction is not null)
            throw new InvalidOperationException("Transaction is already active.");
        _transaction = await _shopDbContext.Database.BeginTransactionAsync();
    }

    public void ClearChangeTracker()
    {
        _shopDbContext.ChangeTracker.Clear();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction is null)
            throw new InvalidOperationException("Transaction has not been started");

        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();

        _transaction = null;
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction is null)
            return;

        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();

        _transaction = null;
    }
}