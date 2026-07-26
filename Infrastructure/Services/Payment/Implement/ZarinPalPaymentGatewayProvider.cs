using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Payment.DTOs;
using Application.Features.Payment.DTOs.ZarinPal;
using Application.Features.Payment.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Payment.Implement;

public class ZarinPalPaymentGatewayProvider : PaymentGatewayProviderContract
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly PaymentRepositoryContract _repositoryContract;

    public ZarinPalPaymentGatewayProvider(IConfiguration config, HttpClient httpClient,
        PaymentRepositoryContract repositoryContract)
    {
        _config = config;
        _httpClient = httpClient;
        _repositoryContract = repositoryContract;
    }

    public PaymentGateway Gateway => PaymentGateway.ZarinPal;

    public async Task<PaymentGatewayRequestResult> RequestPaymentAsync(Domain.Entities.Payment payment,
        CreatePaymentDto dto)
    {
        payment.GenerateOrderNumber();
        var requestDto = new CreateZarinPalPaymentDto
        {
            Amount = payment.Amount,
            Description = dto.Description,
            ReferrerId = dto.ReferrerId,
            ZarinPalMetaData = new ZarinPalMetaData
            {
                Mobile = dto.Mobile,
                Email = dto.Email,
                OrderId = payment.ResNum,
                AutoVerify = bool.Parse(_config["Payment:ZarinPal_AutoVerify"])
            }
        };
        var request = new
        {
            merchant_id = _config["Payment:MerchantIdZarinPal"],
            amount = requestDto.Amount,
            description = requestDto.Description,
            callback_url = _config["Payment:ZarinPalCallBackUrl"],
            metadata = requestDto.ZarinPalMetaData
        };

        using var response =
            await _httpClient.PostAsJsonAsync("https://sandbox.zarinpal.com/pg/v4/payment/request.json", request);

        var raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return PaymentGatewayRequestResult.Failed(
                ((int)response.StatusCode).ToString(),
                "خطا در ارتباط با درگاه زرین پال");
        }

        var result = JsonSerializer.Deserialize<ZarinPalResponse>(raw);
        if (string.IsNullOrWhiteSpace(result?.Data?.Authority))
        {
            return PaymentGatewayRequestResult.Failed(
                "INVALID_RESPONSE",
                "پاسخ نامعتبر از درگاه زرین پال");
        }

        var paymentUrl = "https://sandbox.zarinpal.com/pg/StartPay/" + result?.Data.Authority;
        return PaymentGatewayRequestResult.Success(result.Data.Authority, paymentUrl);
    }

    public async Task<VerifyPaymentResult> HandleCallBackAsync(SandBoxCallBackDto dto)
    {
        var payment = await _repositoryContract.GetPaymentByAuthorityAsync(dto.Authority);

        if (payment is null)
            return VerifyPaymentResult.Failed("تراکنش یافت نشد.");

        if (dto.Status != "OK")
        {
            payment.Edit("NOK", null, null, 0);
            payment.MarkAsFailed();
            await _repositoryContract.SaveAsync();

            return VerifyPaymentResult.Failed("پرداخت توسط کاربر لغو شد.");
        }

        if (payment.PaymentStatus == PaymentStatus.Success)
        {
            return VerifyPaymentResult.Success(
                $"این تراکنش قبلاً تایید شده است. شماره پیگیری: {payment.RefNum}");
        }

        return await VerifyPaymentAsync(dto.Authority);
    }


    public async Task<VerifyPaymentResult> VerifyPaymentAsync(string authority)
    {
        var payment = await _repositoryContract.GetPaymentByAuthorityAsync(authority);

        if (payment is null)
            return VerifyPaymentResult.Failed("تراکنش یافت نشد");

        var verifyRequest = new
        {
            merchant_id = _config["Payment:MerchantIdZarinPal"],
            amount = payment.Amount,
            authority = authority
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                _config["Payment:ZarinPalVerifyUrl"],
                verifyRequest);

            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return VerifyPaymentResult.Failed($"خطا از درگاه: {raw}");

            var result = JsonSerializer.Deserialize<ZarinPalVerifyResponse>(raw);

            if (result?.Data is null)
                return VerifyPaymentResult.Failed("پاسخ نامعتبر از درگاه زرین پال");

            if (result.Data.Code == 100)
            {
                payment.Edit("OK", result.Data.RefId, result.Data.CardPan, result.Data.Fee);
                payment.MarkAsSuccess();
                await _repositoryContract.SaveAsync();

                return VerifyPaymentResult.Success(
                    $"تراکنش با شماره پیگیری {result.Data.RefId} انجام شد");
            }

            if (result.Data.Code == 101)
            {
                if (payment.PaymentStatus != PaymentStatus.Success)
                {
                    payment.Edit("OK", result.Data.RefId, result.Data.CardPan, result.Data.Fee);
                    payment.MarkAsSuccess();
                    await _repositoryContract.SaveAsync();
                }

                return VerifyPaymentResult.Success(
                    $"این تراکنش قبلاً تایید شده است. شماره پیگیری: {result.Data.RefId}");
            }

            payment.Edit("NOK", null, null, 0);
            payment.MarkAsFailed();
            await _repositoryContract.SaveAsync();

            return VerifyPaymentResult.Failed("تایید تراکنش توسط درگاه انجام نشد.");
        }
        catch (Exception)
        {
            return VerifyPaymentResult.Failed("خطای غیر منتظره در ارتباط با درگاه.");
        }
    }
}