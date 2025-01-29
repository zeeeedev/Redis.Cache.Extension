
using System;
using System.Collections.Generic;
using System.Linq;
using Redis.Cache.Extension.Extensions;

namespace Redis.Cache.Extension.Helpers
{
    public static class CacheKeyHelper
    {
        public static string GetCacheKey(string url, string body, string method)
        {
            var items = new List<string>() { method, url, body }.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.ToUpperInvariant());

            if (!items.Any())
            {
                return string.Empty;
            }

            var hashCode = string.Join("|", items).GetDeterministicHashCode();
            string prefix = string.Empty;
            try
            {
                var urlBuilder = new UriBuilder(url);
                var prefixItems = urlBuilder.Path.Split('/').Where(x => !string.IsNullOrWhiteSpace(x));
                prefix = string.Join("|", prefixItems);
                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    prefix += "|";
                }
            }
            catch { }

            return $"{prefix}{hashCode}";
        }
    }
}
