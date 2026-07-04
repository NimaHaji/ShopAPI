
namespace Application.Features.Payment.DTOs.Saman;

public class VerifySamanPayment
{
    public TransactionDetail? TransactionDetail { get; set; }
    public int? ResultCode { get; set; }
    public string? ResultDescription { get; set; }
    public bool Success { get; set; }
        
}

public class TransactionDetail
{
    public string? RRN { get; set; }
    public string? RefNum { get; set; }
    public string? MaskedPan { get; set; }
    public string? HashedPan { get; set; }
}