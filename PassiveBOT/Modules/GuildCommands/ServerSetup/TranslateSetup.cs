namespace PassiveBOT.Modules.GuildCommands.ServerSetup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.Commands;

    using PassiveBOT.Discord.Context;
    using PassiveBOT.Discord.Preconditions;
    using PassiveBOT.Models;

    /// <summary>
    /// The translate setup module
    /// </summary>
    [Group("TranslateSetup")]
    [RequireContext(ContextType.Guild)]
    [RequireAdmin]
    public class TranslateSetup : Base
    {
        /// <summary>
        /// Tutorial on translation command usage
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("HowTo")]
        [Summary("HowTo")]
        [Remarks("QuickTranslate Command Help")]
        public async Task HowTo()
        {
            await SimpleEmbedAsync("**Translation Help**\n" +
                                   "What is QuickTranslation?\n" +
                                   "Quick translation enables you to react to a message with a specific emoji and have it auto-translated into a specific language.\n" +
                                   "You can add an Emoji and Set its language using the Add Command and Remove one using the Remove Command\n" +
                                   "The Quick Translation system can be toggled using the Toggle Command\n\n" +
                                   "**Usage**\n" +
                                   $"`{Context.Prefix}Translation Add <Emoji> <Language>` - Adds a quick translation configuration\n" +
                                   $"`{Context.Prefix}Translation Add :flag_us: en` - Reacting with the :flag_us: emoji will translate the message to english\n\n" +
                                   $"`{Context.Prefix}Translation Remove <Emoji>` - Removes a quick translation configuration\n" +
                                   $"`{Context.Prefix}Translation Remove :flag_us:` - Removed the custom configuration\n" +
                                   $"`{Context.Prefix}Translation Toggle` - Toggles on or off Quick Translation via reactions\n" +
                                   $"`{Context.Prefix}Translation List` - List the Custom Configuration\n" +
                                   $"`{Context.Prefix}Translation Defaults` - List the Default Configuration\n\n" +
                                   "NOTE: For a list of Language Codes\n" +
                                   $"`{Context.Prefix}Translate languages`\n" +
                                   "Also, \n" +
                                   "for `zh-CN - Chinese(Simplified)` use `zh_CN`\n" +
                                   "for `zh-TW - Chinese(Traditional)` use `zh_TW`\n" +
                                   "for `is - Icelandic` use `_is`");
        }

        /// <summary>
        /// Toggle quick translation in the server
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("Toggle")]
        [Summary("Toggle")]
        [Remarks("Toggle quick translation for the server")]
        public async Task Toggle()
        {
            Context.Server.Settings.Translate.EasyTranslate = !Context.Server.Settings.Translate.EasyTranslate;
            Context.Server.Save();
            await SimpleEmbedAsync($"Quick Translate Enabled: {Context.Server.Settings.Translate.EasyTranslate}");
        }

        /// <summary>
        /// toggle dm translations in the server
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("TranslateDM")]
        [Summary("TranslateDM")]
        [Remarks("Toggle whether or not Translations will be sent in DMs")]
        public async Task ToggleMD()
        {
            Context.Server.Settings.Translate.DMTranslations = !Context.Server.Settings.Translate.DMTranslations;
            Context.Server.Save();
            await SimpleEmbedAsync($"DM Quick Translations: {Context.Server.Settings.Translate.DMTranslations}");
        }

        /// <summary>
        /// Removes a custom quick translation emote.
        /// </summary>
        /// <param name="emote">
        /// The emote.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Throws if there is no matching emote.
        /// </exception>
        [Command("Remove")]
        [Summary("Remove <Emote>")]
        [Remarks("Remove a custom emote from QuickTranslation list")]
        public async Task Remove(Emoji emote)
        {
            var matchingPair = Context.Server.Settings.Translate.CustomPairs.FirstOrDefault(x => x.EmoteMatches.Contains(emote.Name));
            if (matchingPair != null)
            {
                if (matchingPair.EmoteMatches.Count == 1)
                {
                    Context.Server.Settings.Translate.CustomPairs.Remove(matchingPair);
                }
                else
                {
                    matchingPair.EmoteMatches.Remove(emote.Name);
                }
            }
            else
            {
                throw new Exception("No Matching Pair.");
            }

            Context.Server.Save();
            await SimpleEmbedAsync("Removed.");
        }

        /// <summary>
        /// Adds a quick translate pair.
        /// </summary>
        /// <param name="emote">
        /// The input emote.
        /// </param>
        /// <param name="languageCode">
        /// The languageCode.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Throws if a pair is already matched.
        /// </exception>
        [Command("Add")]
        [Summary("Add <Emote> <LanguageCode>")]
        [Remarks("Add a pair for quick translation of a language")]
        public async Task QuickTranslatePair(Emoji emote, LanguageMap.LanguageCode languageCode)
        {
            var group = Context.Server.Settings.Translate.CustomPairs.FirstOrDefault(x => x.Language == languageCode);
            if (group != null)
            {
                if (group.EmoteMatches.Any(x => x == emote.Name))
                {
                    throw new Exception("Emote already mapped to a language");
                }

                group.EmoteMatches.Add(emote.Name);
            }
            else
            {
                Context.Server.Settings.Translate.CustomPairs.Add(new GuildModel.GuildSetup.TranslateSetup.TranslationSet
                {
                    EmoteMatches = new List<string>
                    {
                        emote.Name
                    },
                    Language = languageCode
                });
            }

            Context.Server.Save();
            await SimpleEmbedAsync("Pair Added:\n" +
                                   $"{emote.Name} => {languageCode}");
        }

        /// <summary>
        /// The custom emotes list
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("List")]
        [Summary("List")]
        [Remarks("List paired languages")]
        public async Task List()
        {
            var fields = Context.Server.Settings.Translate.CustomPairs.Select(x => new EmbedFieldBuilder
            {
                Name = x.Language.ToString(),
                Value = string.Join("\n", x.EmoteMatches),
                IsInline = true
            }).ToList();
            var embed = new EmbedBuilder
            {
                Fields = fields
            };
            await ReplyAsync(embed);
        }

        /// <summary>
        /// The default emotes list
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("Defaults")]
        [Summary("Defaults")]
        [Remarks("List Default paired languages")]
        public async Task ListDe()
        {
            var fields = LanguageMap.DefaultMap.OrderByDescending(x => x.EmoteMatches.Count).Select(x => new EmbedFieldBuilder
            {
                Name = x.Language.ToString(),
                Value = string.Join("\n", x.EmoteMatches),
                IsInline = true
            }).ToList();
            var embed = new EmbedBuilder
            {
                Fields = fields
            };
            await ReplyAsync(embed);
        }
    }
}