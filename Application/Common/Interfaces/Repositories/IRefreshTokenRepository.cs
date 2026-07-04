using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetAsync(string token);
    Task AddAsync(RefreshToken token);
    Task SaveChangesAsync();
    Task<List<RefreshToken?>> GetRefreshTokensByIdAsync(Guid userId);
}