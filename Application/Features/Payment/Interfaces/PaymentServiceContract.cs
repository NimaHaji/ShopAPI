using Application.Features.Payment.DTOs;
using Application.Features.Payment.DTOs.ZarinPal;
using Domain.Entities;


namespace Application.Features.Payment.Interfaces;

public interface PaymentServiceContract
{
    Task<string?> CreatePaymentAsync(CreatePaymentDto dto);
    Task<VerifyPaymentResult> HandleCallBackAsync(PaymentGateway gateway,SandBoxCallBackDto dto);
}