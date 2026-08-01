using System.Security.Cryptography;
using Domain.Services;

namespace Infrastructure.Services.Sku;

public class SkuGenerator:SkuGeneratorContract
{
    public string GenerateSku()
    {
        Span<byte> bytes=stackalloc byte[6];
        RandomNumberGenerator.Fill(bytes);

        return $"PRD-{Convert.ToHexString(bytes)}";
    }
}