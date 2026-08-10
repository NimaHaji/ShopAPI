using Application.Features.IdempotencyKey.DTOs;

namespace Application.Features.IdempotencyKey.Interfaces;

public interface IdempotencyServiceContract
{
    Task<IdempotencyResultDto> CheckAsync(Guid userId, string key);
    Task CompleteAsync(Guid userId, string key,Guid orderId);
}