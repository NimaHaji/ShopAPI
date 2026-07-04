using Domain.Entities;

namespace Application.Common.Services;

public class RefreshTokenService
{
    private readonly IHasher _hasher;

    public RefreshTokenService(IHasher hasher)
    {
        _hasher = hasher;
    }
    public string HashToken(string token)
        => _hasher.Hash(token);
}