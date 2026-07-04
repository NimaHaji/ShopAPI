using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateJwtToken(User user);
    string GenerateRefreshToken();
}