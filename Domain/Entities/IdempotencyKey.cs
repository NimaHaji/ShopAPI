using Shared.Exceptions;

namespace Domain.Entities;

public class IdempotencyKey
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }
    public User User { get; private set; }

    public string Key { get; private set; }

    public IdempotencyOperation IdempotencyOperation { get; private set; }
    public Guid? ResourceId { get; private set; }

    public IdempotencyStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private IdempotencyKey()
    {
    }

    public IdempotencyKey(Guid userId, string key, IdempotencyOperation idempotencyOperation)
    {
        if (userId == Guid.Empty)
            throw new BusinessException("شناسه کاربری نا معتبر است .");
        
        if (string.IsNullOrEmpty(key))
            throw new BusinessException("کلید جلوگیری از ثبت تکراری (Idempotency-Key) نمی‌تواند خالی باشد.");

        Id = Guid.NewGuid();
        IdempotencyOperation=idempotencyOperation;
        UserId = userId;
        Key = key;
        CreatedAt = DateTime.UtcNow;
        Status = IdempotencyStatus.Processing;
    }

    public void Complete(Guid resourceId)
    {
        if (Status != IdempotencyStatus.Processing)
            throw new BusinessException("Only a processing idempotency key can be completed.");
        
        if (resourceId == Guid.Empty)
            throw new BusinessException("شناسه نا معتبر است .");
        
        ResourceId = resourceId;
        Status = IdempotencyStatus.Completed;
    }
}