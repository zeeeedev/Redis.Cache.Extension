using System;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Redis.Cache.Extension.Helpers;
using Redis.Cache.Extension.Models;
using Redis.Cache.Extension.Services;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private static object _lock = new object();

        public static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetSection("Redis.Cache").Get<CacheConfig>() ?? new CacheConfig();
            services.AddSingleton(config);

            if (config == null || !config.IsEnabled)
            {
                services.AddSingleton<ICacheService, NoCacheService>();
                return services;
            }

            Cache.EnableCache();
            switch (config.Type)
            {
                case CacheType.Redis:
                    SetRedisConnectionString(config);
                    RegisterRedisCache(services, config.ConnectionString);
                    break;
                case CacheType.Memory:
                default:
                    services.AddMemoryCache();
                    services.AddSingleton<ICacheService, MemoryCacheService>();
                    break;
            }

            return services;
        }

        private static void RegisterRedisCache(IServiceCollection services, string connectionString)
        {
            var connection = ConnectionMultiplexer.Connect(connectionString);
            services.AddSingleton<IConnectionMultiplexer>(connection);
            services.AddSingleton(connection.GetDatabase());
            services.AddSingleton<ICacheService, RedisCacheService>();
        }

        private static void SetRedisConnectionString(CacheConfig config)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Development;
            if (environment == Environments.Development)
            {
                config.ConnectionString = "localhost:6379";

                return;
            }

            var client = new AmazonSimpleSystemsManagementClient();
            var request = new GetParameterRequest()
            {
                Name = $"/Redis.Cache.Extension/{environment}/Redis/ConnectionString"
            };

            lock (_lock)
            {
                try
                {
                    var response = client.GetParameterAsync(request).GetAwaiter().GetResult();
                    config.ConnectionString = response.Parameter.Value;
                }
                catch (ParameterNotFoundException exception)
                {
                    throw new ApplicationException($"Redis connection string for {environment} does not exist in AWS Parameter Store for Redis.Cache.Extension. Contact Customer Platform team for assistance.", exception);
                }
            }
        }
    }
}
