using Domain.Enums;

namespace Application.Features.Payment.Interfaces;

public interface PaymentGatewayResolverContract
{
    PaymentGatewayProviderContract Resolve(PaymentGateway gateway);
}