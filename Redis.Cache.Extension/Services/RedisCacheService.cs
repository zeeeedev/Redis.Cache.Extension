using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redis.Cache.Extension.Helpers;
using Redis.Cache.Extension.Models;
using Redis.Cache.Extension.Extensions;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redis.Cache.Extension.Services
{
    public class RedisCacheService : BaseCacheService, ICacheService
    {
        private readonly IConnectionMultiplexer _connection;
        private readonly IDatabase _cache;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly int _keySplitLimit = 50000;

        public RedisCacheService(
            ILogger<RedisCacheService> logger,
            IConnectionMultiplexer connection,
            IDatabase cache,
            CacheConfig cacheConfig,
            IHostEnvironment environment)
             : base(cacheConfig, environment)
        {
            _logger = logger;
            _connection = connection;
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

            var cacheKey = GetCacheKey(key, applicationName);
            string value = null;
            try
            {
                var redisKey = new RedisKey(cacheKey);
                value = await _cache.StringGetAsync(redisKey);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred while getting cached item. Key: {cacheKey}");
            }

            if (value is null)
            {
                return default;
            }

            // We store No content results as string with just space in it so its not interpretted as null and thus result in a cache miss.
            // In this case just return empty string
            if (string.IsNullOrWhiteSpace(value))
            {
                return (T)(object)string.Empty;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(value, JsonSerialization.SerializerSettings);
            }
            catch
            {
                return (T)(object)value;
            }
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

            var cacheKey = GetCacheKey(key);

            var cacheValue = value is string ? value as string : JsonConvert.SerializeObject(value, typeof(T), JsonSerialization.SerializerSettings);
            var expiry = slidingExpiration != null ? slidingExpiration : absoluteExpiration;
            if (expiry == null || expiry == TimeSpan.MinValue || expiry == TimeSpan.MaxValue)
            {
                expiry = _absoluteExpiration;
            }

            var redisKey = new RedisKey(cacheKey);
            var redisValue = new RedisValue(cacheValue);

            try
            {
                await _cache.StringSetAsync(redisKey, redisValue, expiry, When.Always, CommandFlags.FireAndForget);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred while adding item in cache. Key: {cacheKey}, Value: {cacheValue}");
            }
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

            var cacheKey = GetCacheKey(key);
            try
            {
                var redisKey = new RedisKey(cacheKey);
                await _cache.KeyDeleteAsync(redisKey, CommandFlags.FireAndForget);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred while removing cached item. Key: {cacheKey}");
            }
        }

        public async Task RemoveByPatternAsync(IEnumerable<string> keys, string applicationName = null)
        {
            await RemoveByPatternAsync(GetCacheKey(keys), applicationName);
        }

        public async Task RemoveByPatternAsync(string key, string applicationName = null)
        {
            if (!_isEnabled || string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            var cacheKey = GetCacheKey(key, applicationName);
            var pattern = $"{cacheKey}*";
            try
            {
                var keys = new List<RedisKey>();
                var servers = _connection.GetServers();
                foreach (var server in servers)
                {
                    keys.AddRange(await server.KeysAsync(pattern: pattern).ToListAsync());
                }

                if (keys?.Any() == true)
                {
                    // We are splitting keys over 50k to ensure that all keys are deleted in one single connection because we have fire and forget settings.
                    // This is ensure millions of keys can be cleaned without causing connection exceptions.
                    var keysSplit = keys.ChunkBy(_keySplitLimit).ToList();
                    keysSplit.ForEach(keyList =>
                    {
                        using (var connection = ConnectionMultiplexer.Connect(_connectionString))
                        {
                            var db = connection.GetDatabase();
                            keyList.ToList().ForEach(async x => await db.KeyDeleteAsync(x, CommandFlags.FireAndForget));
                        }
                    });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred while removing cached item by pattern match. Key Pattern: {pattern}");
            }
        }
    }
}
