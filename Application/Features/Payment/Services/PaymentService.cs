using System.Net.Http.Json;
using System.Security.Cryptography;
using Application.Features.Order.Interfaces;
using Application.Features.Payment.DTOs;
using Application.Features.Payment.DTOs.ZarinPal;
using Application.Features.Payment.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Payment.Services;

public class PaymentService : PaymentServiceContract
{
    private readonly PaymentRepositoryContract _paymentRepository;
    private readonly PaymentGatewayResolverContract _gatewayResolver;
    private readonly OrderRepositoryContract _orderRepository;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public PaymentService(PaymentRepositoryContract paymentRepository, IConfiguration configuration,
        HttpClient httpClient, PaymentGatewayResolverContract gatewayResolver, OrderRepositoryContract orderRepository)
    {
        _paymentRepository = paymentRepository;
        _configuration = configuration;
        _httpClient = httpClient;
        _gatewayResolver = gatewayResolver;
        _orderRepository = orderRepository;
    }
    public async Task<string?> CreatePaymentAsync(CreatePaymentDto dto)
    {
        var order = await _orderRepository.GetOrderByIdAsync(dto.OrderId);
        if (order is null)
            return "سفارشی یافت نشد";
        
        var payment = new Domain.Entities.Payment(order.TotalPrice,dto.Description, dto.Gateway);

        var provider = _gatewayResolver.Resolve(dto.Gateway);

        var requestResult = await provider.RequestPaymentAsync(payment, dto);

        if (!requestResult.IsSuccess)
        {
            payment.MarkAsFailed();
            await _paymentRepository.SaveAsync();

            throw new InvalidOperationException(requestResult.ErrorMessage);
        }
        payment.Authority=requestResult.GatewayToken;
        await _paymentRepository.CreatePaymentAsync(payment);
        await _paymentRepository.SaveAsync();
        return requestResult.PaymentUrl;
    }

    public async Task<VerifyPaymentResult> HandleCallBackAsync(PaymentGateway gateway, SandBoxCallBackDto dto)
    {
        var provider = _gatewayResolver.Resolve(gateway);
        var result= await provider.HandleCallBackAsync(dto);
        return result;
    }
}