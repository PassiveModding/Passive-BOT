using System.Collections.Generic;
using System.Linq;

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
    }
}