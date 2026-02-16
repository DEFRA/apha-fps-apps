using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;


namespace Apha.Common.Utilities.Cache
{
    public class CacheService : ICacheService
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
    }
}
