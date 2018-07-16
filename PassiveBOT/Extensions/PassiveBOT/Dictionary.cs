namespace PassiveBOT.Extensions.PassiveBOT
{
    using System.Collections.Concurrent;

    public static class ConcurrentDictionary
    {
        public static ConcurrentDictionary<T, int> TryIncrementOrAdd<T>(this ConcurrentDictionary<T, int> source, T key, int increment = 1)
        {
            if (source.TryGetValue(key, out _))
            {
                source[key] += increment;
            }
            else
            {
                source.TryAdd(key, increment);
            }

            return source;
        }
    }
}