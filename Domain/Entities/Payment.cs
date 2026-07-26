using Domain.Enums;

namespace Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public string? State { get; set; }
    public long Amount { get; set; }
    public string? Wage { get; set; }
    public string ResNum { get; set; }
    public string Description { get; set; }
    public string? RefNum { get; set; }

    public string? TraceNo { get; set; }

    public string? RRN { get; set; }

    public string? CardNumber { get; set; }
    public string? Authority { get; set; }

    public PaymentStatus PaymentStatus { get; set; }
    public string? PaymentGatewayStatus { get; set; }
    public PaymentGateway Gateway { get; set; }
    public string? SecurePan { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public Payment(long amount, string description, PaymentGateway gateway)
    {
        Id = Guid.NewGuid();
        Amount = amount;
        Gateway = gateway;
        PaymentStatus = PaymentStatus.pending;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public void Edit(string? paymentGatewayStatus, string? state, string? _RRN, string? refNum, string? resNum,
        string? traceNo, string? wage)
    {
        PaymentGatewayStatus = paymentGatewayStatus;
        State = state;
        RRN = _RRN;
        RefNum = refNum;
        ResNum = resNum;
        TraceNo = traceNo;
        Wage = wage;
    }

    public void Edit(string? paymentGatewayStatus, int? refNum, string? securePan, int? fee)
    {
        PaymentGatewayStatus = paymentGatewayStatus;
        RefNum = refNum.ToString();
        SecurePan = securePan;
        Wage = fee.ToString();
    }

    void SetPaidAt()
    {
        PaidAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        PaymentStatus = PaymentStatus.Failed;
    }

    public void MarkAsSuccess()
    {
        if (PaymentStatus == PaymentStatus.Success)
            return;

        PaymentStatus = PaymentStatus.Success;
        PaidAt ??= DateTime.UtcNow;
    }

    public void GenerateOrderNumber()
    {
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        string shortGuid = Guid.NewGuid().ToString("N").Substring(0, 8);
        ResNum = $"{timestamp}{shortGuid}";
    }
}

public enum PaymentStatus
{
    pending,
    Success,
    Failed,
    Expired
}