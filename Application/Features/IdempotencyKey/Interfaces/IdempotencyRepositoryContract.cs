namespace Application.Features.IdempotencyKey.Interfaces;

public interface IdempotencyRepositoryContract
{
    Task<Domain.Entities.IdempotencyKey?> GetAsync(Guid userId, string key);
    Task<bool> ExistsAsync(Guid userId, string key);
    Task AddAsync(Domain.Entities.IdempotencyKey idempotencyKey);
}