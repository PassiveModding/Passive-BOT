using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Discord.Context;
using PassiveBOT.Discord.Preconditions;
using PassiveBOT.Models;

namespace PassiveBOT.Modules.GuildSetup
{
    [Group("Translation")]
    [RequireContext(ContextType.Guild)]
    [RequireAdmin]
    public class EasyTranslate : Base
    {
        [Command("HowTo")]
        [Summary("HowTo")]
        [Remarks("QuickTranslate Howto")]
        public async Task Howto()
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
                                   $"`{Context.Prefix}Translation Toggle` - Toggles on or off quicktranslation\n" +
                                   $"`{Context.Prefix}Translation List` - List the Custom Configuration\n" +
                                   $"`{Context.Prefix}Translation Defaults` - List the Default Configuration\n\n" +
                                   "NOTE: For a list of Language Codes\n" +
                                   $"`{Context.Prefix}Translate languages`\n" +
                                   "Also, \n" +
                                   "for `zh-CN - Chinese(Simplified)` use `zh_CN`\n" +
                                   "for `zh-TW - Chinese(Traditional)` use `zh_TW`\n" +
                                   "for `is - Icelandic` use `_is`");
        }

        [Command("Toggle")]
        [Summary("Toggle")]
        [Remarks("Toggle quick translation for the server")]
        public async Task Toggle()
        {
            Context.Server.Settings.Translate.EasyTranslate = !Context.Server.Settings.Translate.EasyTranslate;
            Context.Server.Save();
            await SimpleEmbedAsync($"Quick Translate Enabled: {Context.Server.Settings.Translate.EasyTranslate}");
        }

        [Command("TranslateDM")]
        [Summary("TranslateDM")]
        [Remarks("Toggle wether or not Translations will be sent in DMs")]
        public async Task ToggleMD()
        {
            Context.Server.Settings.Translate.DMTranslations = !Context.Server.Settings.Translate.DMTranslations;
            Context.Server.Save();
            await SimpleEmbedAsync($"DM Quick Translations: {Context.Server.Settings.Translate.DMTranslations}");
        }

        [Command("Remove")]
        [Summary("Remove <Emote>")]
        [Remarks("Remove a custom emote from QuickTranslation list")]
        public async Task Remove(Emoji RemoveEmote)
        {
            var matchingpair = Context.Server.Settings.Translate.Custompairs.FirstOrDefault(x => x.EmoteMatches.Contains(RemoveEmote.Name));
            if (matchingpair != null)
            {
                if (matchingpair.EmoteMatches.Count == 1)
                {
                    Context.Server.Settings.Translate.Custompairs.Remove(matchingpair);
                }
                else
                {
                    matchingpair.EmoteMatches.Remove(RemoveEmote.Name);
                }
            }
            else
            {
                throw new Exception("No Matching Pair.");
            }

            Context.Server.Save();
            await SimpleEmbedAsync("Removed.");
        }

        [Command("Add")]
        [Summary("Add <Emote> <LanguageCode>")]
        [Remarks("Add a pair for quick translation of a language")]
        public async Task QuickTranslatePair(Emoji InputEmote, LanguageMap.languagecode languagepair)
        {
            var languagegroup = Context.Server.Settings.Translate.Custompairs.FirstOrDefault(x => x.Language == languagepair);
            if (languagegroup != null)
            {
                if (languagegroup.EmoteMatches.Any(x => x == InputEmote.Name))
                {
                    throw new Exception("Emote already mapped to a language");
                }

                languagegroup.EmoteMatches.Add(InputEmote.Name);
            }
            else
            {
                Context.Server.Settings.Translate.Custompairs.Add(new GuildModel.gsettings.translate.TObject
                {
                    EmoteMatches = new List<string>
                    {
                        InputEmote.Name
                    },
                    Language = languagepair
                });
            }

            Context.Server.Save();
            await SimpleEmbedAsync("Pair Added:\n" +
                                   $"{InputEmote.Name} => {languagepair}");
        }

        [Command("List")]
        [Summary("List")]
        [Remarks("List paired languages")]
        public async Task List()
        {
            var fields = Context.Server.Settings.Translate.Custompairs.Select(x => new EmbedFieldBuilder
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

        [Command("Defaults")]
        [Summary("Defaults")]
        [Remarks("List Default paired languages")]
        public async Task ListDe()
        {
            var fields = LanguageMap.Map.OrderByDescending(x => x.EmoteMatches.Count).Select(x => new EmbedFieldBuilder
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