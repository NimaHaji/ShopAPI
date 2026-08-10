using Domain.Entities;

namespace Application.Features.IdempotencyKey.DTOs;

public class IdempotencyResultDto
{
    public Guid OrderId { get; set; }   
    public IdempotencyStatus Status { get; init; }
}
