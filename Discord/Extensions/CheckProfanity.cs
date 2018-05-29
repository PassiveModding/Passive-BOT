using System.Globalization;
using System.Linq;
using System.Text;

namespace PassiveBOT.Discord.Extensions
{
    public class CheckProfanity
    {
        //profanity
        public static string[] Profanity =
        {
            "porn",
            "nigger",
            "retard",
            "shit",
            "fuck",
            "rape",
            "cunt",
            "slut",
            "penis",
            "dick",
            "vagina",
            "xxx",
            "asshole",
            "arsehole",
            "bitch",
            "blowjob",
            "boob",
            "tits",
            "titties",
            "breast",
            "dildo",
            "gay",
            "whore",
            "jizz",
            "kkk",
            "nigga",
            "pussy",
            "sex"
        };

        public static bool ContainsProfanity(string input)
        {
            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark) stringBuilder.Append(c);
            }

            var cleanedstring = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();

            return Profanity.Any(x => cleanedstring.Contains(x));
        }
    }
}