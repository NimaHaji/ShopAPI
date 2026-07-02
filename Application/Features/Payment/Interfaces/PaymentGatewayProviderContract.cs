using Application.Features.Payment.DTOs;
using Domain.Entities;

namespace Application.Features.Payment.Interfaces;

public interface PaymentGatewayProviderContract
{
    public PaymentGateway Gateway { get;}
    
    Task<PaymentGatewayRequestResult> RequestPaymentAsync(Domain.Entities.Payment payment, CreatePaymentDto dto);
    
    Task<VerifyPaymentResult> HandleCallBackAsync(SandBoxCallBackDto dto);
    Task<VerifyPaymentResult> VerifyPaymentAsync(string verifyAuthority);
}