using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Common.Interfaces;

public interface UnitOfWorkContract
{
    Task<int> SaveAsync();
    Task BeginTransactionAsync();
    void ClearChangeTracker();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}