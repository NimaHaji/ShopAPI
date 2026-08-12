using Domain.Entities;

namespace Application.Features.IdempotencyKey.Interfaces;

public interface IdempotencyRepositoryContract
{
    Task<Domain.Entities.IdempotencyKey?> GetAsync(Guid userId, string key,IdempotencyOperation operation);
    Task<bool> ExistsAsync(Guid userId, string key, IdempotencyOperation operation);
    Task AddAsync(Domain.Entities.IdempotencyKey idempotencyKey);
}