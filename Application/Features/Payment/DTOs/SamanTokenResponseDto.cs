using System.Text.Json.Serialization;

namespace Application.Features.Payment.DTOs;

public class SamanTokenResponseDto
{   
    [JsonPropertyName("status")]
    public int Status { get; set; }
    [JsonPropertyName("token")]
    public string? Token { get; set; }
    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }
    [JsonPropertyName("errorDesc")]
    public string? ErrorDescription { get; set; }
}