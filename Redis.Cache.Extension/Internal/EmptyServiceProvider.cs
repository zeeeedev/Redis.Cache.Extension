using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Redis.Cache.Extension.Internal
{
    internal class EmptyServiceProvider : IServiceProvider
    {
        private readonly Lazy<MethodInfo> _emptyFactory = new Lazy<MethodInfo>(() => typeof(Enumerable).GetMethod("Empty", BindingFlags.Static | BindingFlags.Public));

        public object GetService(Type serviceType)
        {
            if (!serviceType.IsGenericType || !(typeof(IEnumerable<>) == serviceType.GetGenericTypeDefinition()))
            {
                return null;
            }

            return _emptyFactory.Value.MakeGenericMethod(serviceType.GetGenericArguments()[0]).Invoke(null, null);
        }
    }
}
