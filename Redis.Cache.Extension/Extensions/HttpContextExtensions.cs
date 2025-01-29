using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Redis.Cache.Extension.Helpers;

namespace Redis.Cache.Extension.Extensions
{
    public static class HttpContextExtensions
    {
        public static async Task<string> GetCacheKey(this HttpContext context)
        {
            var url = context.Request.GetUrlString();
            var body = await context.Request.GetRequestBodyString();
            var method = context.Request.Method;

            return CacheKeyHelper.GetCacheKey(url, body, method);
        }
    }
}
