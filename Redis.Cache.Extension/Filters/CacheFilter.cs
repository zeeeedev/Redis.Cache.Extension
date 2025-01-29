using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using Redis.Cache.Extension.Extensions;
using Redis.Cache.Extension.Helpers;
using Redis.Cache.Extension.Services;

namespace Redis.Cache.Extension.Filters
{
    public class CacheFilter : IAsyncActionFilter
    {
        private ICacheService _cache;
        private bool _isCacheDisabled = true;
        private TimeSpan? _absoluteExpiration { get; set; }
        private TimeSpan? _slidingExpiration { get; set; }

        public CacheFilter(ICacheService cache, string[] expirations)
        {
            _cache = cache;
            _isCacheDisabled = cache is NoCacheService;
            if (TimeSpan.TryParse(expirations?.ElementAtOrDefault(0), out TimeSpan absoluteExpiration))
            {
                _absoluteExpiration = absoluteExpiration;
            }
            if (TimeSpan.TryParse(expirations?.ElementAtOrDefault(1), out TimeSpan slidingExpiration))
            {
                _slidingExpiration = slidingExpiration;
            }
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (_isCacheDisabled)
            {
                // Do nothing if cache is disabled
                await next();

                return;
            }

            var cacheKey = await context.HttpContext.GetCacheKey();
            var responseObject = await _cache.GetAsync<ObjectResult>(cacheKey);

            if (responseObject == null)
            {
                // Cache miss
                var actionExecutedContext = await next();
                var result = actionExecutedContext.Result as IStatusCodeActionResult;
                if (result != null && result.StatusCode >= 200 && result.StatusCode < 300)
                {
                    await _cache.SetAsync(cacheKey, actionExecutedContext.Result, _absoluteExpiration, _slidingExpiration);
                }
            }
            else
            {
                // Cache hit
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = (int)(responseObject as IStatusCodeActionResult).StatusCode;
                if (responseObject.Value != null)
                {
                    var responseBody = JsonConvert.SerializeObject(responseObject.Value, responseObject.Value.GetType(), JsonSerialization.SerializerSettings);
                    await context.HttpContext.Response.WriteAsync(responseBody);
                }
            }
        }
    }
}