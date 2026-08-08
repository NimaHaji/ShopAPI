using Domain.Enums;

namespace Domain.Entities;

public class Payment
{
    public Guid Id { get; private set; }
    public string? State { get; private set; }
    public long Amount { get; private set; }
    public string? Wage { get; private set; }
    public string ResNum { get; private set; }
    public string Description { get; private set; }
    public string? RefNum { get; private set; }

    public string? TraceNo { get; private set; }

    public string? RRN { get; private set; }

    public string? CardNumber { get; private set; }
    public string? Authority { get; private set; }

    public PaymentStatus PaymentStatus { get; private set; }
    public string? PaymentGatewayStatus { get; private set; }
    public PaymentGateway Gateway { get; private set; }
    public string? SecurePan { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? PaidAt { get; private set; }

    public Order Order { get; private set; }
    public Guid OrderId { get; private set; }

    public Payment(long amount, string description, PaymentGateway gateway, Guid orderId)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
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

    public void SetAuthority(string? gatewayToken)
    {
        Authority = gatewayToken;
    }
}

public enum PaymentStatus
{
    pending,
    Success,
    Failed,
    Expired
}