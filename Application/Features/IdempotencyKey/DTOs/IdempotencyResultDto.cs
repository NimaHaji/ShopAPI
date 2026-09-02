using Domain.Entities;

namespace Application.Features.IdempotencyKey.DTOs;

public class IdempotencyResultDto
{
    public Guid? ResourceId { get; set; }   
    public IdempotencyStatusDto Status { get; set; }
    public IdempotencyOperation Operation { get; set; }
}

public enum IdempotencyStatusDto
{
    New ,
    Processing,
    Completed
}
