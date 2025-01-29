using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redis.Cache.Extension.Services
{
    public interface ICacheService
    {
        Task<T> GetAsync<T>(IEnumerable<string> keys, string applicationName = null);
        Task<T> GetAsync<T>(string key, string applicationName = null);
        Task SetAsync<T>(IEnumerable<string> keys, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);
        Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);
        Task RemoveAsync(IEnumerable<string> keys);
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(IEnumerable<string> keys, string applicationName = null);
        Task RemoveByPatternAsync(string key, string applicationName = null);
    }
}
