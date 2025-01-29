using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Redis.Cache.Extension.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Redis.Cache.Extension.Services
{
    public class MemoryCacheService : BaseCacheService, ICacheService
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheService(IMemoryCache cache, CacheConfig cacheConfig, IHostEnvironment environment) : base(cacheConfig, environment)
        {
            _cache = cache;
        }

        public async Task<T> GetAsync<T>(IEnumerable<string> keys, string applicationName = null)
        {
            return await GetAsync<T>(GetCacheKey(keys), applicationName);
        }

        public async Task<T> GetAsync<T>(string key, string applicationName = null)
        {
            if (!_isEnabled || string.IsNullOrWhiteSpace(key))
            {
                return default;
            }

            return await Task.FromResult(_cache.Get<T>(key));
        }

        public async Task SetAsync<T>(IEnumerable<string> keys, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            await SetAsync(GetCacheKey(keys), value, absoluteExpiration, slidingExpiration);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            if (!_isEnabled || string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration != null && absoluteExpiration > TimeSpan.Zero ? absoluteExpiration : _absoluteExpiration,
                SlidingExpiration = slidingExpiration != null && slidingExpiration > TimeSpan.Zero ? slidingExpiration : _slidingExpiration
            };

            await Task.FromResult(_cache.Set(key, value, options));
        }

        public async Task RemoveAsync(IEnumerable<string> keys)
        {
            await RemoveAsync(GetCacheKey(keys));
        }

        public async Task RemoveAsync(string key)
        {
            if (!_isEnabled || string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            await Task.Run(() => _cache.Remove(key));
        }

        public async Task RemoveByPatternAsync(IEnumerable<string> keys, string applicationName = null)
        {
            await RemoveByPatternAsync(GetCacheKey(keys));
        }

        public async Task RemoveByPatternAsync(string key, string applicationName = null)
        {
            if (!_isEnabled || string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            var keys = GetAllKeys(key);
            if (keys?.Any() == true)
            {
                foreach (var x in keys)
                {
                    await Task.Run(() => _cache.Remove(x));
                }
            }
        }

        /// <summary>
        /// Get all keys that starts with the key provided
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private List<string> GetAllKeys(string key)
        {
            var coherentState = typeof(MemoryCache).GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance);
            var coherentStateValue = coherentState.GetValue(_cache);
            var entriesCollection = coherentStateValue.GetType().GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            var entriesCollectionValue = entriesCollection.GetValue(coherentStateValue) as ICollection;
            var keys = new List<string>();

            if (entriesCollectionValue?.Count > 0)
            {
                foreach (var item in entriesCollectionValue)
                {
                    var methodInfo = item.GetType().GetProperty("Key");
                    var existingKey = methodInfo.GetValue(item).ToString();
                    if (existingKey.StartsWith(key))
                    {
                        keys.Add(existingKey);
                    }
                }
            }

            return keys;
        }
    }
}
