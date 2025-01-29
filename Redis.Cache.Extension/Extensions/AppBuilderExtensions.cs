using Microsoft.Extensions.DependencyInjection;
using Redis.Cache.Extension.Helpers;
using Redis.Cache.Extension.Middlewares;
using Redis.Cache.Extension.Models;

namespace Microsoft.AspNetCore.Builder
{
    public static class AppBuilderExtensions
    {
        public static IApplicationBuilder UseCache(this IApplicationBuilder app)
        {
            var config = app.ApplicationServices.GetService<CacheConfig>();
            if (config != null && config.IsEnabled)
            {
                CacheServiceLocator.SetServiceProvider(app.ApplicationServices);
            }

            app.UseMiddleware<BeginRequestMiddleware>();

            return app;
        }
    }
}
