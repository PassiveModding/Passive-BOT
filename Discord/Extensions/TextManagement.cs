using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace PassiveBOT.Discord.Extensions
{
    public class TextManagement
    {
        /// <summary>
        ///     Split a list into a group of lists of a specified size.
        /// </summary>
        /// <typeparam name="T">Type of item held within the list</typeparam>
        /// <param name="FullList">Input list</param>
        /// <param name="GroupSize">Size of Groups to output</param>
        /// <returns></returns>
        public static List<List<T>> splitList<T>(List<T> FullList, int GroupSize = 30)
        {
            var newlist = new List<List<T>>();
            for (var i = 0; i < FullList.Count; i += GroupSize)
            {
                newlist.Add(FullList.Skip(i).Take(GroupSize).ToList());
            }

            return newlist;
        }

        public static string GetInvite(Context.Context context)
        {
            return GetInvite(context.Client);
        }

        public static string GetInvite(IDiscordClient client)
        {
            return $"https://discordapp.com/oauth2/authorize?client_id={client.CurrentUser.Id}&scope=bot&permissions=2146958591";
        }

        // This code is an implementation of the pseudocode from the Wikipedia,
        // showing a naive implementation.
        // You should research an algorithm with better space complexity.
        public static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {
                return n;
            }
            for (int i = 0; i <= n; d[i, 0] = i++)
                ;
            for (int j = 0; j <= m; d[0, j] = j++)
                ;
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }
    }
}