namespace PassiveBOT.Extensions
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Discord;
    using Discord.WebSocket;

    using global::PassiveBOT.Models;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json.Linq;

    /// <summary>
    ///     The translate methods.
    /// </summary>
    public class TranslateMethods
    {
        /// <summary>
        ///     The converts a google translate response into a single output string
        /// </summary>
        /// <param name="input">
        ///     The input.
        /// </param>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public static string HandleResponse(JArray input)
        {
            var stringList = input[0].Select(section => section[0].ToString()).ToList();

            return string.Join(string.Empty, stringList);
        }

        /// <summary>
        ///     Converts from a language code to a useable google translate code.
        /// </summary>
        /// <param name="code">
        ///     The code.
        /// </param>
        /// <returns>
        ///     The <see cref="string" />.
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
        ///     Simple method for translating a message with an embed response
        /// </summary>
        /// <param name="language">
        ///     The language code
        /// </param>
        /// <param name="provider">
        ///     The provider.
        /// </param>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="reaction">
        ///     The reaction.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public static async Task<EmbedBuilder> TranslateEmbedAsync(LanguageMap.LanguageCode language, IServiceProvider provider, IUserMessage message, SocketReaction reaction = null)
        {
            var embed = new EmbedBuilder { Title = "Translate", Color = Color.Blue };
            var original = message.Content.FixLength();
            var languageString = LanguageCodeToString(language);
            var file = await TranslateMessageAsync(languageString, message.Content, provider);
            var response = HandleResponse(file).FixLength();
            embed.AddField($"Translated [{language}{(reaction?.Emote == null ? string.Empty : $"{reaction.Emote}")}]", $"{response}");
            embed.AddField($"Original [{file[2]}]", $"{original}");
            embed.Footer = new EmbedFooterBuilder { Text = $"Original Author: {message.Author}{(reaction == null ? string.Empty : $" || Reactor: {reaction.User.Value}")}", IconUrl = reaction.User.Value.GetAvatarUrl() };
            return embed;
        }

        /// <summary>
        ///     Translates the input message to the desired language
        /// </summary>
        /// <param name="language">
        ///     The language.
        /// </param>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="provider">
        ///     The provider.
        /// </param>
        /// <returns>
        ///     The response file
        /// </returns>
        public static async Task<JArray> TranslateMessageAsync(string language, string message, IServiceProvider provider)
        {
            var client = provider.GetRequiredService<HttpClient>();

            // https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl=ru&dt=t&ie=UTF-8&oe=UTF-8&q=hi there this is a test message
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={language}&dt=t&ie=UTF-8&oe=UTF-8&q={Uri.EscapeDataString(message)}";

            var content = await client.GetStringAsync(url);
            var file = JArray.Parse(content);
            return file;
        }
    }
}