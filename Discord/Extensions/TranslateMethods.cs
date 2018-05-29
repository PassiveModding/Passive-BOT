using System.Collections.Generic;

namespace PassiveBOT.Discord.Extensions
{
    public class TranslateMethods
    {
        public static string HandleReponse(dynamic input)
        {
            //input[0][0][0].ToString();
            var stringlist = new List<string>();
            foreach (var section in input[0])
            {
                stringlist.Add(section[0].ToString());
            }

            return string.Join("", stringlist);
        }
    }
}