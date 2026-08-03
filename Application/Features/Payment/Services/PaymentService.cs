using Application.Common.Interfaces;
using Application.Features.Order.Interfaces;
using Application.Features.Payment.DTOs;
using Application.Features.Payment.Interfaces;
using Domain.Enums;
using Shared.Exceptions;

namespace Application.Features.Payment.Services;

public class PaymentService : PaymentServiceContract
{
    private readonly PaymentRepositoryContract _paymentRepository;
    private readonly PaymentGatewayResolverContract _gatewayResolver;
    private readonly OrderRepositoryContract _orderRepository;
    private readonly IUSerContext _userContext;

    public PaymentService(PaymentRepositoryContract paymentRepository, PaymentGatewayResolverContract gatewayResolver, OrderRepositoryContract orderRepository, IUSerContext userContext)
    {
        _paymentRepository = paymentRepository;
        _gatewayResolver = gatewayResolver;
        _orderRepository = orderRepository;
        _userContext = userContext;
    }
    public async Task<string?> CreatePaymentAsync(CreatePaymentDto dto)
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");
        
        var order = await _orderRepository.GetOrderByIdAsync(dto.OrderId,userId);
        if (order is null)
            throw new NotFoundException("سفارشی یافت نشد"); 
        
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