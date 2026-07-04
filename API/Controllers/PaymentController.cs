using Application.Features.Payment.DTOs;
using Application.Features.Payment.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class PaymentController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly PaymentServiceContract _paymentServiceContract;

    public PaymentController(HttpClient httpClient, IConfiguration config,
        PaymentServiceContract paymentServiceContract)
    {
        _httpClient = httpClient;
        _config = config;
        _paymentServiceContract = paymentServiceContract;
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> GetPaymentUrl([FromBody] CreatePaymentDto dto)
    {
        var paymentUrl = await _paymentServiceContract.CreatePaymentAsync(dto);
        return Ok(paymentUrl);
    }
    private Dictionary<string, string?> GetAllValues()
    {
        var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var q in Request.Query)
            dict[q.Key] = q.Value.FirstOrDefault();

        if (Request.HasFormContentType)
        {
            foreach (var f in Request.Form)
                dict[f.Key] = f.Value.FirstOrDefault();
        }

        return dict;
    }
    [HttpPost("{gateway}")]
    [HttpGet("{gateway}")]
    [Authorize]
    public async Task<IActionResult> CallBack([FromRoute] PaymentGateway gateway)
    {
        var values = GetAllValues();

        values.TryGetValue("Amount", out var amountStr);

        var dto = new SandBoxCallBackDto
        {
            Authority = values.GetValueOrDefault("Authority"),
            Status = values.GetValueOrDefault("Status"),
            State = values.GetValueOrDefault("State"),
            Amount = long.TryParse(amountStr, out var amount) ? amount : null,
            RRN = values.GetValueOrDefault("RRN"),
            RefNum = values.GetValueOrDefault("RefNum"),
            ResNum = values.GetValueOrDefault("ResNum"),
            TraceNo = values.GetValueOrDefault("TraceNo"),
            Wage = values.GetValueOrDefault("Wage"),
            SecurePan = values.GetValueOrDefault("SecurePan"),
            CID = values.GetValueOrDefault("CID"),
            Token = values.GetValueOrDefault("Token")
        };

        var result = await _paymentServiceContract.HandleCallBackAsync(gateway, dto);

        return Ok(result);
    }
}