using Application.Features.Payment.DTOs;
using Domain.Enums;

namespace Application.Features.Payment.Interfaces;

public interface PaymentServiceContract
{
    Task<string?> CreatePaymentAsync(CreatePaymentDto dto,string idempotencyKey);
    Task<VerifyPaymentResult> HandleCallBackAsync(PaymentGateway gateway,SandBoxCallBackDto dto);
}