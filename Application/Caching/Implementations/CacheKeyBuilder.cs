using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Caching.Interfaces;
using Application.Features.Product.DTOs;

namespace Application.Caching.Implementations;

public sealed class CacheKeyBuilder : CacheKeyBuilderContract
{
    #region Product

    public string Product(Guid id)
        => $"products:{id}";

    public string ProductList(ProductQueryDto query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var json = JsonSerializer.Serialize(query);
        var hash = ComputeHash(json);

        return $"products:list:{hash}";
    }

    public string ProductListPattern()
        => "products:list:*";

    public string ProductSearch(string search)
    {
        var normalizedSearch = NormalizeSearch(search);
        var hash = ComputeHash(normalizedSearch);

        return $"products:search:{hash}";
    }

    public string ProductSearchPattern()
        => "products:search:*";

    #endregion

    #region Category

    public string Category(Guid id)
        => $"categories:{id}";

    public string CategoryList()
        => "categories:list";

    public string CategorySearch(string search)
    {
        var normalizedSearch = NormalizeSearch(search);
        var hash = ComputeHash(normalizedSearch);

        return $"categories:search:{hash}";
    }

    public string CategorySearchPattern()
        => "categories:search:*";

    #endregion

    #region Brand

    public string Brand(Guid id) => "brands:{id}";

    public string BrandList() => "brands:list";

    public string BrandSearch(string search)
    {
        var normalizedSearch = NormalizeSearch(search);
        var hash = ComputeHash(normalizedSearch);

        return $"brands:search:{hash}";
    }

    public string BrandSearchPattern() => "brands:search:*";

    #endregion

    #region Review

    public string ReviewList(Guid productId) => $"reviews:product:{productId}";

    #endregion

    #region Helpers

    private static string NormalizeSearch(string search)
        => search.Trim().ToLowerInvariant();

    private static string ComputeHash(string value)
        => Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(value)));

    #endregion
}