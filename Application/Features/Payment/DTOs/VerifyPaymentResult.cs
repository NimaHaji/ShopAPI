namespace Application.Features.Payment.DTOs;

public class VerifyPaymentResult
{
    public bool IsSuccess { get; set; }
    public string? RefNumber { get; set; }
    public string Message { get; set; }
    
    public static VerifyPaymentResult Success(string message)
    {
        return new VerifyPaymentResult
        {
            IsSuccess = true,
            Message = message
        };
    }
    public static VerifyPaymentResult Failed(string message)
    {
        return new VerifyPaymentResult
        {
            IsSuccess = false,
            Message = message
        };
    }
}