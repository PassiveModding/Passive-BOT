namespace PassiveBOT.Discord.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    
    using global::Discord;
    using global::Discord.WebSocket;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json.Linq;

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
        public static string HandleResponse(JArray input)
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
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <returns>
        /// The response file
        /// </returns>
        public static async Task<JArray> TranslateMessage(string language, string message, IServiceProvider provider)
        {
            var client = provider.GetRequiredService<HttpClient>();

            // https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl=ru&dt=t&ie=UTF-8&oe=UTF-8&q=hi there this is a test message
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={language}&dt=t&ie=UTF-8&oe=UTF-8&q={Uri.EscapeDataString(message)}";

            var content = await client.GetStringAsync(url);
            var file = JArray.Parse(content);
            return file;
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

        /// <summary>
        /// Simple method for translating a message with an embed response
        /// </summary>
        /// <param name="language">
        /// The language code
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="reaction">
        /// The reaction.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task<EmbedBuilder> TranslateEmbed(LanguageMap.LanguageCode language, IServiceProvider provider, SocketUserMessage message, SocketReaction reaction = null)
        {
            var embed = new EmbedBuilder { Title = "Translate", Color = Color.Blue };
            var original = TextManagement.FixLength(message.Content);
            var languageString = LanguageCodeToString(language);
            var file = await TranslateMessage(languageString, message.Content, provider);
            var response = TextManagement.FixLength(HandleResponse(file));
            embed.AddField($"Translated [{language}{(reaction?.Emote == null ? "" : $"{reaction.Emote}")}]", $"{response}");
            embed.AddField($"Original [{file[2]}]", $"{original}");
            embed.Footer = new EmbedFooterBuilder
                               {
                                   Text = $"Original Author: {message.Author}{(reaction == null ? "" : $" || Reactor: {reaction.User.Value}")}",
                                   IconUrl = reaction.User.Value.GetAvatarUrl()
                               };
            return embed;
        }
    }
}