namespace PassiveBOT.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Google.Cloud.Translation.V2;

    using Discord;
    using Discord.WebSocket;

    using global::PassiveBOT.Models;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json.Linq;

    using Raven.Client.Documents.Linq.Indexing;

    /// <summary>
    ///     The translate methods.
    /// </summary>
    public class TranslateMethodsNew
    {
        public TranslateMethodsNew(DatabaseObject config, TranslateLimitsNew limits, ConfigModel model)
        {
            Config = config;
            if (config.TranslateAPIKey != null)
            {
                Client = TranslationClient.CreateFromApiKey(Config.TranslateAPIKey);
            }
            else
            {
                Client = null;
            }

            Limits = limits;

            Model = model;
        }

        public ConfigModel Model { get; set; }

        public TranslateLimitsNew Limits { get; set; }

        public DatabaseObject Config { get; set; }

        public TranslationClient Client { get; set; }

        public class TranslationResponse
        {
            public TranslationResponse(TranslationResult result, TranslateLimitsNew.ResponseStatus status, string responseMessage)
            {
                Response = result;
                AuthenticationResponse = status;
                ResponseMessage = responseMessage;
            }

            public TranslationResult Response { get; set; }

            public TranslateLimitsNew.ResponseStatus AuthenticationResponse { get; set; }

            public string ResponseMessage { get; set; }
        }

        public async Task<TranslationResponse> TranslateTextAsync(string message, IGuildChannel channel, LanguageMap.LanguageCode destination, LanguageMap.LanguageCode? source = null)
        {
            if (Client == null)
            {
                return new TranslationResponse(null, TranslateLimitsNew.ResponseStatus.Error, $"Translation is not enabled with this instance of the bot.");
            }

            if (message == null)
            {
                return new TranslationResponse(null, TranslateLimitsNew.ResponseStatus.Error, "Message is null");
            }

            if (channel?.Guild == null)
            {
                return new TranslationResponse(null, TranslateLimitsNew.ResponseStatus.Error, "Translation can only be invoked in a discord server.");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return new TranslationResponse(new TranslationResult("", "", "", "", "", null), TranslateLimitsNew.ResponseStatus.GuildSucceded, "Message Empty");
            }

            try
            {
                var authResponse = await Limits.UpdateAsync(channel.GuildId, message);
                if (authResponse == TranslateLimitsNew.ResponseStatus.GuildSucceded)
                {
                    var response = await Client.TranslateTextAsync(message, LanguageCodeToString(destination), LanguageCodeToString(source));
                    return new TranslationResponse(response, authResponse, "Success");
                }

                if (authResponse == TranslateLimitsNew.ResponseStatus.GuildLimitExceeded)
                {
                    return new TranslationResponse(null, authResponse, $"Guild does not have any available translations left. {Model.GetTranslateUrl()}");
                }

                if (authResponse == TranslateLimitsNew.ResponseStatus.GuildLimitExceededByMessage)
                {
                    return new TranslationResponse(null, authResponse, $"Message is too long and will exceed the remaining characters available for translation in the guild. {Model.GetTranslateUrl()}");
                }
                
                return new TranslationResponse(null, TranslateLimitsNew.ResponseStatus.Error, "There was an error retrieving guild data.");
            }
            catch (Exception e)
            {
                return new TranslationResponse(null, TranslateLimitsNew.ResponseStatus.Error, $"Something went wrong while translating...\n{e}");
            }
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
        public static string LanguageCodeToString(LanguageMap.LanguageCode? code)
        {
            if (code == null)
            {
                return null;
            }

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

        public async Task<EmbedBuilder> TranslateEmbedAsync(LanguageMap.LanguageCode language, IUserMessage message, IGuildChannel channel, SocketReaction reaction = null)
        {
            var embed = new EmbedBuilder { Title = "Translate", Color = Color.Blue };
            var original = message.Content.FixLength();
            var file = await TranslateTextAsync(message.Content, channel, language);
            if (file.AuthenticationResponse == TranslateLimitsNew.ResponseStatus.GuildSucceded)
            {
                var response = file.Response.TranslatedText.FixLength();
                embed.AddField($"Translated [{language}{(reaction?.Emote == null ? string.Empty : $"{reaction.Emote}")}]", $"{response}");
                embed.AddField($"Original [{file.Response.DetectedSourceLanguage}]", $"{original}");
                embed.Footer = new EmbedFooterBuilder { Text = $"Original Author: {message.Author}{(reaction == null ? string.Empty : $" || Reactor: {reaction.User.Value}")}", IconUrl = reaction.User.Value.GetAvatarUrl() };
                return embed;
            }

            throw new Exception("Guild error");
        }

        public async Task<(string, Embed)> TranslateFullMessageAsync(LanguageMap.LanguageCode language, IUserMessage message, IGuildChannel channel, SocketReaction reaction = null)
        {
            var newMessage = await TranslateTextAsync(message.Content ?? "", channel, language);

            if (newMessage.AuthenticationResponse != TranslateLimitsNew.ResponseStatus.GuildSucceded)
            {
                Console.WriteLine("Failed");
                return (newMessage.ResponseMessage, null);
            }


            EmbedBuilder newBuilder = null;
            if (message.Embeds.Any())
            {
                var first = message.Embeds.FirstOrDefault();
                if (first != null)
                {
                    try
                    {
                        var originalBuilder = first.ToEmbedBuilder();
                        newBuilder = new EmbedBuilder();
                        int length = originalBuilder.Length;
                        if (Limits.Guilds.TryGetValue(channel?.GuildId ?? 0, out var guild))
                        {
                            if (guild.CanTranslate(length + message.Content?.Length ?? 0))
                            {
                                if (originalBuilder.Title != null)
                                {
                                    newBuilder.Title = (await TranslateTextAsync(originalBuilder.Title, channel, language)).Response.TranslatedText.FixLength(256);
                                }

                                foreach (var field in originalBuilder.Fields)
                                {
                                    var newField = new EmbedFieldBuilder();
                                    newField.IsInline = field.IsInline;
                                    newField.Name = (await TranslateTextAsync(field.Name, channel, language)).Response.TranslatedText.FixLength(256);
                                    newField.Value = (await TranslateTextAsync(field.Value.ToString(), channel, language)).Response.TranslatedText.FixLength(1024);
                                    newBuilder.AddField(newField);
                                }

                                if (originalBuilder.Description != null)
                                {
                                    newBuilder.Description = (await TranslateTextAsync(originalBuilder.Description, channel, language)).Response.TranslatedText.FixLength(2048);
                                }

                                newBuilder.ImageUrl = originalBuilder.ImageUrl;
                                newBuilder.ThumbnailUrl = originalBuilder.ThumbnailUrl;
                                newBuilder.Color = originalBuilder.Color;
                                newBuilder.Author = originalBuilder.Author;
                                if (originalBuilder.Footer != null)
                                {
                                    newBuilder.Footer = new EmbedFooterBuilder
                                                            {
                                                                IconUrl = originalBuilder.Footer?.IconUrl ?? "",
                                                                Text = originalBuilder.Footer?.Text == null ? "" : (await TranslateTextAsync(originalBuilder.Footer.Text, channel, language)).Response.TranslatedText.FixLength(2048)
                                                            };
                                }
                                
                                Console.WriteLine("Embed method");
                                return (newMessage.Response.TranslatedText.FixLength(), newBuilder.Build());
                            }

                            return (newMessage.Response.TranslatedText.FixLength(2048), new EmbedBuilder { Description = $"Unable to translate entire embed due to exceeding translation quota, {Model.GetTranslateUrl()}" }.Build());
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception");
                        return (e.ToString().FixLength(), null);
                    }
                }
            }
            
            Console.WriteLine("End of method");
            return (newMessage.Response.TranslatedText.FixLength(), null);
        }
    }
}