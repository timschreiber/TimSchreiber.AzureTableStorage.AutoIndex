using System.Collections.Generic;
using System.Linq;

namespace TimSchreiber.EnumerableExtensions
{
    public static class Extensions
    {
        public static IEnumerable<IEnumerable<T>> InChunksOf<T>(this IEnumerable<T> input, int chunkSize)
        {
            return input.Select((x, i) => new { Index = i, Value = x })
                .Where(x => x.Value != null)
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value));
        }
    }
}
