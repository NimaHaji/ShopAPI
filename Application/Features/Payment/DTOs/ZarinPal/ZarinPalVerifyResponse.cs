using System.Text.Json.Serialization;

namespace Application.Features.Payment.DTOs.ZarinPal;

public class ZarinPalVerifyResponse
{
    [JsonPropertyName("data")]
    public ZarinPalVerifyData Data { get; set; }
}

public class ZarinPalVerifyData
{
    [JsonPropertyName("code")]
    public int Code { get; set; }
    [JsonPropertyName("card_pan")]
    public string CardPan { get; set; }
    [JsonPropertyName("ref_id")]
    public int RefId { get; set; }
    [JsonPropertyName("card_hash")]
    public string CardHash { get; set; }
    [JsonPropertyName("fee")]
    public int Fee { get; set; }
}