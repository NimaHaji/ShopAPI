using Application.Features.Checkout.DTOs;

namespace Application.Features.Checkout.Interfaces;

public interface CheckoutServiceContract
{
    Task<Guid> CheckoutAsync(CheckoutDto dto);
}