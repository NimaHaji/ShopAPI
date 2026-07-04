using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository:IRefreshTokenRepository
{
    private readonly ShopDbContext _context;

    public RefreshTokenRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task AddAsync(RefreshToken token)
    {
        await _context.RefreshTokens.AddAsync(token);
        await SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<RefreshToken>?> GetRefreshTokensByIdAsync(Guid userId)
    {
        return await _context
            .RefreshTokens
            .Where(x=>x.UserId == userId && !x.IsRevoked)
            .ToListAsync();
    }
}