using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Payment.DTOs;
using Application.Features.Payment.DTOs.Saman;
using Application.Features.Payment.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Payment.Implement;

public class SamanPaymentGatewayProvider : PaymentGatewayProviderContract
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly PaymentRepositoryContract _paymentRepositoryContract;

    public SamanPaymentGatewayProvider(HttpClient httpClient, IConfiguration configuration, PaymentRepositoryContract paymentRepositoryContract)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _paymentRepositoryContract = paymentRepositoryContract;
    }

    public PaymentGateway Gateway => PaymentGateway.Saman;

    public async Task<PaymentGatewayRequestResult> RequestPaymentAsync(Domain.Entities.Payment payment, CreatePaymentDto dto)
    {
        var paymentPageUrl = "https://sandbox.banktest.ir/saman/sep.shaparak.ir/OnlinePG/SendToken";
        payment.GenerateOrderNumber();
        var dtoRequest = new SamanTokenRequestDto()
        {
            Action = "token",
            Amount = payment.Amount,
            ResNum = payment.ResNum,
        };
        var request = new
        {
            dtoRequest.Action,
            TerminalId = _configuration["Payment:TerminalId"]!,
            dtoRequest.Amount,
            dtoRequest.ResNum,
            RedirectUrl = _configuration["Payment:SamanRedirectUrl"],
            dtoRequest.CellNumber
        };
        using var response = await _httpClient.PostAsJsonAsync(
            "https://sandbox.banktest.ir/saman/sep.shaparak.ir/OnlinePG/OnlinePG",
            request);

        var raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return PaymentGatewayRequestResult.Failed(
                ((int)response.StatusCode).ToString(),
                "خطا در ارتباط با درگاه سامان");
        }

        var result = JsonSerializer.Deserialize<SamanTokenResponseDto>(raw);

        if (result is null || string.IsNullOrWhiteSpace(result.Token))
        {
            return PaymentGatewayRequestResult.Failed(
                "INVALID_RESPONSE",
                "پاسخ نامعتبر از درگاه سامان");
        }

        var paymentUrl = $"{paymentPageUrl}?token={result.Token}";
        return PaymentGatewayRequestResult.Success(result.Token, paymentUrl);
    }

    public async Task<VerifyPaymentResult> HandleCallBackAsync(SandBoxCallBackDto dto)
    {
        var payment=await _paymentRepositoryContract.GetPaymentByResNumAsync(dto.ResNum);
        var request = new
        {
            RefNum = dto.RefNum,
            TerminalNumber = _configuration["Payment:TerminalId"]
        };
        var verifyUrl = _configuration["Payment:VerifyTransactionUrl"];
        
        var response = await _httpClient.PostAsJsonAsync(verifyUrl, request);
            
        var result=await response.Content.ReadFromJsonAsync<VerifySamanPayment>();
        
        if (!response.IsSuccessStatusCode)
            return VerifyPaymentResult.Failed("تراکنش ناموفق بود");
        
        return VerifyPaymentResult.Success(result.ResultDescription);
    }

    public Task<VerifyPaymentResult> VerifyPaymentAsync(string verifyAuthority)
    {
        throw new NotImplementedException();
    }
}