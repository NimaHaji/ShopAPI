using Application.Features.IdempotencyKey.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class IdempotencyRepository:IdempotencyRepositoryContract
{
    private readonly ShopDbContext _shopDbContext;

    public IdempotencyRepository(ShopDbContext shopDbContext)
    {
        _shopDbContext = shopDbContext;
    }

    public async Task<IdempotencyKey?> GetAsync(Guid userId, string key)
    {
        return await _shopDbContext
            .IdempotencyKeys
            .Where(ik => 
                ik.UserId == userId &&
                ik.Key == key)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(Guid userId, string key)
    {
        return await _shopDbContext
            .IdempotencyKeys
            .Where(ik => ik.UserId == userId && ik.Key == key)
            .AnyAsync();
    }

    public async Task AddAsync(IdempotencyKey idempotencyKey)
    {
        await _shopDbContext
            .IdempotencyKeys
            .AddAsync(idempotencyKey);
    }
}