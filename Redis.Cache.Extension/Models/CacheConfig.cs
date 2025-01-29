using System;

namespace Redis.Cache.Extension.Models
{
    public class CacheConfig
    {
        /// <summary>
        /// Turn caching on or off
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Cache type
        /// </summary>
        public CacheType Type { get; set; } = CacheType.Memory;

        /// <summary>
        /// Cache key delimiter
        /// </summary>
        /// <value></value>
        public string Delimiter { get; } = "|";

        /// <summary>
        /// Cache absolute expiration
        /// </summary>
        /// <returns></returns>
        public TimeSpan AbsoluteExpiration { get; set; } = new TimeSpan(1, 0, 0);

        /// <summary>
        /// Cache sliding expiration
        /// </summary>
        /// <value></value>
        public TimeSpan? SlidingExpiration { get; set; } = null;

        /// <summary>
        /// Redis connection string
        /// </summary>
        /// <value></value>
        internal string ConnectionString { get; set; } = null;
    }
}
