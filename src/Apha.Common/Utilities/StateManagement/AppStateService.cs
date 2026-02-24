using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;


namespace Apha.Common.Utilities.StateManagement
{
    public class CacheService : IAppStateService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDistributedCache _cache;            

        public CacheService(
            IHttpContextAccessor httpContextAccessor,
            IDistributedCache cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;            
        }

        private ISession? Session =>
        _httpContextAccessor.HttpContext?.Session;

        #region Cache        
        public async Task SetCacheValueAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(60)
                };

                var json = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, json, options);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<T?> GetCacheValueAsync<T>(string key)
        {
            try
            {
                var json = await _cache.GetStringAsync(key);
                if (json is null) return default;

                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RemoveCacheValueAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Session 
        public Task SetSessionAsync<T>(string key, T value)
        {
            if (Session == null) return Task.CompletedTask;

            Session.Set(key, JsonSerializer.SerializeToUtf8Bytes(value));
            return Task.CompletedTask;
        }

        public Task<T?> GetSessionAsync<T>(string key)
        {
            if (Session == null) return Task.FromResult<T?>(default);

            return Session.TryGetValue(key, out var data)
                ? Task.FromResult(JsonSerializer.Deserialize<T>(data))
                : Task.FromResult<T?>(default);
        }

        public Task RemoveSessionAsync(string key)
        {
            Session?.Remove(key);
            return Task.CompletedTask;
        }

        #endregion
    }
}
