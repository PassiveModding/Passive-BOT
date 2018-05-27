using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PassiveBOT.Discord.Extensions
{
    public class TextManagement
    {
        public static List<List<T>> splitList<T>(List<T> FullList, int GroupSize = 30)
        {
            var newlist = new List<List<T>>();
            for (int i = 0; i < FullList.Count; i += GroupSize)
            {
                newlist.Add(FullList.Skip(i).Take(GroupSize).ToList());
            }

            return newlist;
        }
    }
}
