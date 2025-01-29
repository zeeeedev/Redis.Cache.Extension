using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Redis.Cache.Extension.Models;

namespace Redis.Cache.Extension.Services
{
    public abstract class BaseCacheService
    {
        private readonly IList<string> _cachePrefixKeys;
        private readonly string _cachePrefixKey;
        private readonly string _delimiter;
        protected bool _isEnabled = false;
        protected TimeSpan _absoluteExpiration;
        protected TimeSpan? _slidingExpiration;
        protected readonly string _environmentName;
        protected readonly string _connectionString;

        public BaseCacheService(CacheConfig cacheConfig, IHostEnvironment environment)
        {
            _delimiter = cacheConfig.Delimiter;
            _isEnabled = cacheConfig.IsEnabled;
            _absoluteExpiration = cacheConfig.AbsoluteExpiration;
            _slidingExpiration = cacheConfig.SlidingExpiration;
            _environmentName = environment.EnvironmentName;
            _connectionString = cacheConfig.ConnectionString;
            _cachePrefixKeys = new List<string>()
            {
                environment.ApplicationName?.Replace(".", _delimiter),
                _environmentName
            };
            _cachePrefixKey = string.Join(_delimiter, _cachePrefixKeys);
        }

        protected string GetCacheKey(IEnumerable<string> keys, bool includePrefix = false, string applicationName = null)
        {
            var prefix = new List<string>();

            if (includePrefix)
            {
                if (string.IsNullOrWhiteSpace(applicationName))
                {
                    prefix = prefix.Concat(_cachePrefixKeys).ToList();
                }
                else
                {
                    prefix = prefix.Concat(new List<string>() { applicationName, _delimiter, _environmentName }).ToList();
                }
            }

            var cacheKeys = prefix.Concat(keys);

            return string.Join(_delimiter, cacheKeys);
        }

        protected string GetCacheKey(string key, string applicationName = null)
        {
            var prefix = string.IsNullOrEmpty(applicationName) ? _cachePrefixKey : $"{applicationName}{_delimiter}{_environmentName}";

            return $"{prefix}{_delimiter}{key}".ToUpperInvariant();
        }
    }
}
