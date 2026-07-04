namespace Application.Features.Auth.DTOs;

public record SelectTenantRequestDto(
    string email,
    string password,
    Guid tenantId);