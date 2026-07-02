namespace Application.Features.Payment.Interfaces;

public class PaymentGatewayRequestResult
{
    public bool IsSuccess { get; private set; }
    public string? GatewayToken { get; private set; }
    public string? PaymentUrl { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static PaymentGatewayRequestResult Success(string gatewayToken, string paymentUrl)
        => new()
        {
            IsSuccess = true,
            GatewayToken = gatewayToken,
            PaymentUrl = paymentUrl
        };

    public static PaymentGatewayRequestResult Failed(string? errorCode, string errorMessage)
        => new()
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };
}