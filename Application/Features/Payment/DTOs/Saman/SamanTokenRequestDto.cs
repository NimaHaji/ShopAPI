using System.Text.Json.Serialization;

namespace Application.Features.Payment.DTOs.Saman;

public class SamanTokenRequestDto
{
    [JsonPropertyName("Action")] public string? Action { get; set; } = "token";
    [JsonPropertyName("Amount")] public long Amount { get; set; }
    [JsonPropertyName("Wage")] public decimal? Wage { get; set; }
    [JsonPropertyName("ResNum")] public string? ResNum { get; set; }
    [JsonPropertyName("CellNumber")] public string? CellNumber { get; set; }
    [JsonPropertyName("TokenExpiryInMin")] public int TokenExpiryInMinutes { get; set; }
    [JsonPropertyName("HashedCardNumber")] public string? HashedCardNumber { get; set; }
}