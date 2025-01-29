using System;
using Microsoft.AspNetCore.Mvc;
using Redis.Cache.Extension.Filters;

namespace Redis.Cache.Extension.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class CacheAttribute : TypeFilterAttribute
    {
        public CacheAttribute(params string[] expirations) : base(typeof(CacheFilter))
        {
            Arguments = new[] { expirations };
        }
    }
}
