using Application.Features.Checkout.DTOs;

namespace Application.Features.Checkout.Interfaces;

public interface CheckoutServiceContract
{
    Task<CheckoutResultDto> CheckoutAsync(CheckoutDto dto,string idempotencyKey);
}