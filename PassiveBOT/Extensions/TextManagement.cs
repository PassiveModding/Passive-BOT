namespace PassiveBOT.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Discord;

    /// <summary>
    ///     String Management
    /// </summary>
    public static class TextManagement
    {
        /// <summary>
        ///     Shortens a string to the specified length if it is too long.
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="length">
        ///     The length.
        /// </param>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public static string FixLength(this string message, int length = 1024)
        {
            if (message.Length > length)
            {
                message = message.Substring(0, length - 4) + "...";
            }

            return message;
        }

        /// <summary>
        ///     Split a list into a group of lists of a specified size.
        /// </summary>
        /// <typeparam name="T">Type of item held within the list</typeparam>
        /// <param name="fullList">Input list</param>
        /// <param name="groupSize">Size of Groups to output</param>
        /// <returns>A list of lists of the specified size</returns>
        public static List<List<T>> SplitList<T>(this List<T> fullList, int groupSize = 30)
        {
            var splitList = new List<List<T>>();
            for (var i = 0; i < fullList.Count; i += groupSize)
            {
                splitList.Add(fullList.Skip(i).Take(groupSize).ToList());
            }

            return splitList;
        }

        public static string DecodeBase64(this string original)
        {
            try
            {
                byte[] data = Convert.FromBase64String(original);
                string decodedString = Encoding.UTF8.GetString(data);
                return decodedString;
            }
            catch (Exception e)
            {
                return original;
            }
            
        }
    }
}