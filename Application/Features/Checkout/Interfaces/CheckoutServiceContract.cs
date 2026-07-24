namespace Application.Features.Checkout.Interfaces;

public interface CheckoutServiceContract
{
    Task<Guid> CheckoutAsync();
}