using System.Collections.Generic;
using System.Linq;

namespace Redis.Cache.Extension.Extensions
{
    public static class ListExceptions
    {
        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
        {
            return source
                .AsEnumerable()
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList());
        }
    }
}