using System.Text.Json.Serialization;

namespace Application.Features.Payment.DTOs.ZarinPal;

public class ZarinPalResponse
{   
    [JsonPropertyName("data")]
    public ZarinPalData? Data { get; set; }
    [JsonPropertyName("errors")]
    public object? Error { get; set; }
}

public class ZarinPalData
{
    [JsonPropertyName("authority")]
    public string Authority { get; set; }
}