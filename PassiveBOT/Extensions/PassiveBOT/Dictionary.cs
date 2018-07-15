namespace PassiveBOT.Extensions.PassiveBOT
{
    using System.Collections.Generic;

    public static class Dictionary
    {
        public static Dictionary<T, int> TryIncrementOrAdd<T>(this Dictionary<T, int> source, T key, int increment = 1)
        {
            if (source.TryGetValue(key, out _))
            {
                source[key] += increment;
            }
            else
            {
                source.Add(key, increment);
            }

            return source;
        }
    }
}
