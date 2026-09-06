namespace Application.Caching.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    
    Task RemoveAsync(string key);
    
    Task RemoveByPatternAsync(string pattern);
    
    Task<bool> ExistAsync(string key);
    
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null);
}