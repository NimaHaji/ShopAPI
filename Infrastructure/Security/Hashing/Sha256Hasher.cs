using System.Security.Cryptography;
using System.Text;
using Domain.Entities;

namespace Infrastructure.Security.Hashing;

public class Sha256Hasher:IHasher
{
    public string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    public bool Verify(string input, string hash)
    {
        var computed = Hash(input);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(computed),
            Convert.FromHexString(input)
        );
    }
}