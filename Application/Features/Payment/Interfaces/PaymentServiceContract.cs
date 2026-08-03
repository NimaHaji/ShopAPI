using Application.Features.Payment.DTOs;
using Domain.Enums;

namespace Application.Features.Payment.Interfaces;

public interface PaymentServiceContract
{
    Task<string?> CreatePaymentAsync(CreatePaymentDto dto);
    Task<VerifyPaymentResult> HandleCallBackAsync(PaymentGateway gateway,SandBoxCallBackDto dto);
}