namespace Apha.Common.Utilities.StateManagement
{
    public interface IAppStateService
    {
        Task SetCacheValueAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task<T?> GetCacheValueAsync<T>(string key);
        Task RemoveCacheValueAsync(string key);

        Task SetSessionAsync<T>(string key, T value);
        Task<T?> GetSessionAsync<T>(string key);
        Task RemoveSessionAsync(string key);
    }
}
