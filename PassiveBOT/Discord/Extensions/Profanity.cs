// ReSharper disable StringLiteralTypo
namespace PassiveBOT.Discord.Extensions
{
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The profanity checker
    /// </summary>
    public class Profanity
    {
        /// <summary>
        /// The list.
        /// </summary>
        private static readonly string[] List =
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

        /// <summary>
        /// True if message contains profanity
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool ContainsProfanity(string input)
        {
            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            var cleanedString = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();

            return List.Any(x => cleanedString.Contains(x));
        }
    }
}