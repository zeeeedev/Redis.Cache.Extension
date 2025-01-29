using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Redis.Cache.Extension.Middlewares
{
    public class BeginRequestMiddleware
    {
        private readonly RequestDelegate _next;

        public BeginRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();
            await _next.Invoke(context);
        }
    }
}