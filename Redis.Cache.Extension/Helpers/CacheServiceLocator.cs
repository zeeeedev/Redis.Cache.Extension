using System;
using Redis.Cache.Extension.Internal;

namespace Redis.Cache.Extension.Helpers
{
    public static class CacheServiceLocator
    {
        internal static IServiceProvider Empty = new EmptyServiceProvider();
        private static IServiceProvider _rootServiceProvider = Empty;

        public static IServiceProvider Current => _rootServiceProvider;

        public static void SetServiceProvider(IServiceProvider serviceProvider) => _rootServiceProvider = serviceProvider;
    }
}
