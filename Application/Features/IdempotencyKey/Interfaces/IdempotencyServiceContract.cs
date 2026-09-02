using Application.Features.IdempotencyKey.DTOs;
using Domain.Entities;

namespace Application.Features.IdempotencyKey.Interfaces;

public interface IdempotencyServiceContract
{
    Task<IdempotencyResultDto> CheckAsync(Guid userId, string key,IdempotencyOperation operation);
    Task CompleteAsync(Guid userId, string key,Guid resource,IdempotencyOperation operation);
}