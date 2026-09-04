using System.Text.Json;
using Application.Caching.Interfaces;
using StackExchange.Redis;

namespace Infrastructure.Services.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly IServer _server;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _server = redis.GetServer(redis.GetEndPoints()[0]);
        _database = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        var keys = _server.Keys(
            database: _database.Database,
            pattern: pattern
        );

        foreach (var key in keys)
        {
            await _database.KeyDeleteAsync(key);
        }
    }

    public async Task<bool> ExistAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null)
    {
        var json = JsonSerializer.Serialize(value);

        if (expiration.HasValue)
        {
            await _database.StringSetAsync(
                key,
                json,
                new Expiration(expiration.Value));

            return;
        }

        await _database.StringSetAsync(
            key,
            json,
            Expiration.Default);
    }
}