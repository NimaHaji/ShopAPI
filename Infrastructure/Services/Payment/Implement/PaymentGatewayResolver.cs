using Application.Features.Payment.Interfaces;
using Domain.Entities;

namespace Infrastructure.Services.Implement;

public class PaymentGatewayResolver:PaymentGatewayResolverContract
{
    private readonly IEnumerable<PaymentGatewayProviderContract> _providers;

    public PaymentGatewayResolver(IEnumerable<PaymentGatewayProviderContract> providers)
    {
        _providers = providers;
    }

    public PaymentGatewayProviderContract Resolve(PaymentGateway gateway)
    {
        var provider = _providers.FirstOrDefault(x => x.Gateway == gateway);

        if (provider is null)
            throw new InvalidOperationException($"no provider found for gateway : {gateway}");
        
        return provider;
    }
}