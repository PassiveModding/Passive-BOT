namespace PassiveBOT.Discord.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using Newtonsoft.Json;
    
    using global::PassiveBOT.Models;

    /// <summary>
    /// The translate methods.
    /// </summary>
    public class TranslateMethods
    {
        /// <summary>
        /// The converts a google translate response into a single output string
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string HandleResponse(dynamic input)
        {
            var stringList = new List<string>();
            foreach (var section in input[0])
            {
                stringList.Add(section[0].ToString());
            }

            return string.Join(string.Empty, stringList);
        }

        /// <summary>
        /// Translates the input message to the desired language
        /// </summary>
        /// <param name="language">
        /// The language.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The response file
        /// </returns>
        public static dynamic TranslateMessage(string language, string message)
        {
            using (var client = new WebClient { Encoding = Encoding.UTF8 })
            {
                var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={language}&dt=t&ie=UTF-8&oe=UTF-8&q={Uri.EscapeDataString(message)}";
                var stream = client.OpenRead(url);
                var reader = new StreamReader(stream ?? throw new InvalidOperationException());
                var content = reader.ReadToEnd();
                dynamic file = JsonConvert.DeserializeObject(content);
                client.Dispose();
                return file;
            }
        }

        /// <summary>
        /// Converts from a language code to a useable google translate code.
        /// </summary>
        /// <param name="code">
        /// The code.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string LanguageCodeToString(LanguageMap.LanguageCode code)
        {
            var language = code.ToString();
            if (language == "zh_CN")
            {
                language = "zh-CN";
            }

            if (language == "zh_TW")
            {
                language = "zh-TW";
            }

            if (language == "_is")
            {
                language = "is";
            }

            return language;
        }
    }
}