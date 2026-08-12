using Application.Common.Interfaces;
using Application.Features.IdempotencyKey.DTOs;
using Application.Features.IdempotencyKey.Interfaces;
using Domain.Entities;
using Shared.Exceptions;

namespace Application.Features.IdempotencyKey.Implementations;

public class IdempotencyService : IdempotencyServiceContract
{
    private readonly IdempotencyRepositoryContract _idempotencyRepositoryContract;

    public IdempotencyService(
        IdempotencyRepositoryContract idempotencyRepositoryContract)
    {
        _idempotencyRepositoryContract =
            idempotencyRepositoryContract;
    }

    public async Task<IdempotencyResultDto> CheckAsync(Guid userId, string key, IdempotencyOperation operation)
    {
        if (userId == Guid.Empty)
            throw new BusinessException("شناسه کاربری نا معتبر است .");

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new BusinessException(
                "کلید جلوگیری از ثبت تکراری (Idempotency-Key) نمی‌تواند خالی باشد.");
        }

        key = key.Trim();

        var existing =
            await _idempotencyRepositoryContract
                .GetAsync(userId, key, operation);

        if (existing is not null)
        {
            return existing.Status switch
            {
                IdempotencyStatus.Completed
                    when existing.ResourceId.HasValue =>
                    new IdempotencyResultDto
                    {
                        Status = IdempotencyStatusDto.Completed,
                        ResourceId = existing.ResourceId.Value,
                        Operation = existing.IdempotencyOperation
                    },

                IdempotencyStatus.Processing =>
                    new IdempotencyResultDto
                    {
                        Status = IdempotencyStatusDto.Processing,
                        Operation = existing.IdempotencyOperation
                    },

                _ => throw new InvalidOperationException(
                    "Unknown idempotency status.")
            };
        }

        var idempotencyKey = new Domain.Entities.IdempotencyKey(userId, key, operation);

        await _idempotencyRepositoryContract.AddAsync(idempotencyKey);

        return new IdempotencyResultDto
        {
            Status = IdempotencyStatusDto.New,
            Operation = operation
        };
    }

    public async Task CompleteAsync(Guid userId, string key, Guid resource, IdempotencyOperation operation)
    {
        if (userId == Guid.Empty)
            throw new BusinessException("شناسه کاربری نا معتبر است .");

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new BusinessException(
                "کلید جلوگیری از ثبت تکراری (Idempotency-Key) نمی‌تواند خالی باشد.");
        }

        if (resource == Guid.Empty)
            throw new BusinessException("شناسه سفارش نامعتبر است.");

        key = key.Trim();

        var existing =
            await _idempotencyRepositoryContract
                .GetAsync(userId, key, operation);

        if (existing is null)
        {
            throw new InvalidOperationException(
                "کلید جلوگیری از ثبت تکراری (Idempotency-Key) یافت نشد.");
        }

        existing.Complete(resource);
    }
}