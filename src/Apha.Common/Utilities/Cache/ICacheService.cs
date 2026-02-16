namespace Apha.Common.Utilities.Cache
{
    public interface ICacheService
    {
        Task SetCacheValueAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task<T?> GetCacheValueAsync<T>(string key);
        Task RemoveCacheValueAsync(string key);
    }
}
