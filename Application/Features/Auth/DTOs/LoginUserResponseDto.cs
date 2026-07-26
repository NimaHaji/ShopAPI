namespace Application.Features.Auth.DTOs;

public record LoginUserResponseDto(
    string? AccessToken,
    string RefreshToken);

public record RefreshTokenRequest(string RefreshToken);