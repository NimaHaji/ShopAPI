using Application.Common;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Security.Hashing;

public class PasswordHasher:IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(null, password);

    public bool Verify(string hashedPassword, string password)
        => _hasher.VerifyHashedPassword(
            null,
            hashedPassword,
            password
        ) == PasswordVerificationResult.Success;
}