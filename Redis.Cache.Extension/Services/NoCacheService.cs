using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redis.Cache.Extension.Services
{
    internal class NoCacheService : ICacheService
    {
        public async Task<T> GetAsync<T>(IEnumerable<string> keys, string applicationName = null)
        {
            return await GetAsync<T>(string.Empty, applicationName);
        }

        public async Task<T> GetAsync<T>(string key, string applicationName = null)
        {
            return await Task.FromResult<T>(default);
        }

        public async Task SetAsync<T>(IEnumerable<string> keys, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            await SetAsync(string.Empty, value, absoluteExpiration, slidingExpiration);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            await Task.CompletedTask;
        }

        public async Task RemoveAsync(IEnumerable<string> keys)
        {
            await RemoveAsync(string.Empty);
        }

        public async Task RemoveAsync(string key)
        {
            await Task.CompletedTask;
        }

        public async Task RemoveByPatternAsync(IEnumerable<string> keys, string applicationName = null)
        {
            await Task.CompletedTask;
        }

        public async Task RemoveByPatternAsync(string key, string applicationName = null)
        {
            await Task.CompletedTask;
        }
    }
}
