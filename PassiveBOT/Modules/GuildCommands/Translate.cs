﻿// ReSharper disable StringLiteralTypo

namespace PassiveBOT.Modules.GuildCommands
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.Preconditions;
    using Discord.Commands;

    using PassiveBOT.Context;
    using PassiveBOT.Extensions;
    using PassiveBOT.Models;

    /// <summary>
    ///     The translate commands
    /// </summary>
    [Group("Translate")]
    [RequireContext(ContextType.Guild)]
    [Summary("Language translation commands and information")]
    public class Translate : Base
    {
        private readonly TranslateLimitsNew Limits;

        public Translate(TranslateLimitsNew limits, TranslateMethodsNew methods)
        {
            Limits = limits;
            Methods = methods;
        }

        public TranslateMethodsNew Methods { get; set; }

        /// <summary>
        ///     The translate cmd.
        /// </summary>
        /// <param name="languageCode">
        ///     The language code.
        /// </param>
        /// <param name="message">
        ///     The message to translate.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Priority(1)]
        [Command(RunMode = RunMode.Async)]
        [RateLimit(12, 2, Measure.Minutes, RateLimitFlags.ApplyPerGuild)]
        [Summary("Translate from one language to another")]
        public async Task TranslateCmdAsync(LanguageMap.LanguageCode languageCode, [Remainder] string message)
        {
            var embed = new EmbedBuilder { Title = "Translate", Color = Color.Blue };
            var original = message.FixLength();
            var file = await Methods.TranslateTextAsync(message, Context.Channel as IGuildChannel, languageCode);
            if (file.AuthenticationResponse == TranslateLimitsNew.ResponseStatus.GuildSucceded)
            {
                embed.AddField($"Translated [{file.Response.TargetLanguage}]", $"{file.Response.TranslatedText.FixLength()}");
                embed.AddField($"Original [{file.Response.DetectedSourceLanguage}]", $"{original.FixLength()}");

                await ReplyAsync(string.Empty, false, embed.Build());
                return;
            }

            await ReplyAndDeleteAsync(file.ResponseMessage, TimeSpan.FromSeconds(20));
        }

        [Priority(0)]
        [Command("Redeem")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Redeem a translation upgrade for your discord account")]
        public async Task RedeemAsync([Remainder] string key)
        {
            var result = await Limits.RedeemKeyAsync(Context.Guild.Id, key);

            if (result.Success)
            {
                Limits.Guilds.TryGetValue(Context.Guild.Id, out TranslateLimitsNew.Guild guild);
                string appendString = null;
                if (guild != null)
                {
                    appendString = $"Remaining Characters: {guild.RemainingCharacters()}";
                }

                await SimpleEmbedAsync($"Success you have been upgraded to unlimited, this is valid for: **{result.ValidFor} characters**\n{appendString}");
                Limits.Save();
            }
            else
            {
                await SimpleEmbedAsync("Error, you were unable to redeem the provided token");
            }
        }

        /// <summary>
        ///     lists translation languages
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Priority(2)]
        [Command("languages", RunMode = RunMode.Async)]
        [Summary("A list of available languages codes to convert between")]
        public async Task TranslateListAsync()
        {
            var embed2 = new EmbedBuilder();
            embed2.AddField("INFORMATION", "Format:\n" + "<Language> <Language Code>\n" + "Example Usage:\n" + "`.p translate <language code> <message>`\n" + "`.p translate es Hi there this will be converted to spanish`");
            embed2.AddField("A", "`af` - Afrikaans\n`sq` - Albanian\n`am` - Amharic\n`ar` - Arabic\n`hy` - Armenian\n`az` - Azeerbaijani\n");
            embed2.AddField("B", "`eu` - Basque\n`be` - Belarusian\n`bn` - Bengali\n`bs` - Bosnian\n`bg` - Bulgarian\n");
            embed2.AddField("C", "`ca` - Catalan\n`ceb` - Cebuano\n`zh_CN` - Chinese(Simplified)\n`zh_TW` - Chinese(Traditional)\n`co` - Corsican\n`hr` - Croatian\n`cs` - Czech\n");
            embed2.AddField("D", "`da` - Danish\n`nl` - Dutch\n");
            embed2.AddField("E", "`en` - English\n`eo` - Esperanto\n`et` - Estonian\n");
            embed2.AddField("F", "`fi` - Finnish\n`fr` - French\n`fy` - Frisian\n");
            embed2.AddField("G", "`gl` - Galician\n`ka` - Georgian\n`de` - German\n`el` - Greek\n`gu` - Gujarati\n");
            embed2.AddField("H", "`ht` - Haitian-Creole\n`ha` - Hausa\n`haw` - Hawaiian\n`iw` - Hebrew\n`hi` - Hindi\n`hmn` - Hmong\n`hu` - Hungarian\n");
            embed2.AddField("I", "`_is` - Icelandic \n`ig` - Igbo\n`id` - Indonesian\n`ga` - Irish\n`it` - Italian\n");
            embed2.AddField("J", "`ja` - Japanese\n`jw` - Javanese\n");
            embed2.AddField("K", "`kn` - Kannada\n`kk` - Kazakh\n`km` - Khmer\n`ko` - Korean\n`ku` - Kurdish\n`ky` - Kyrgyz\n");
            embed2.AddField("L", "`lo` - Lao\n`la` - Latin\n`lv` - Latvian\n`lt` - Lithuanian\n`lb` - Luxembourgish\n");
            embed2.AddField("M", "`mk` - Macedonian\n`mg` - Malagasy\n`ms` - Malay\n`ml` - Malayalam\n`mt` - Maltese\n`mi` - Maori\n`mr` - Marathi\n`mn` - Mongolian\n`my` - Myanmar(Burmese)\n");
            embed2.AddField("N", "`ne` - Nepali\n`no` - Norwegian\n`ny` - Nyanja(Chichewa)\n");
            embed2.AddField("P", "`ps` - Pashto\n`fa` - Persian\n`pl` - Polish\n`pt` - Portuguese\n`pa` - Punjabi\n");
            embed2.AddField("R", "`ro` - Romanian\n`ru` - Russian\n");
            embed2.AddField("S", "`sm` - Samoan\n`gd` - Scots-Gaelic\n`sr` - Serbian\n`st` - Sesotho\n`sn` - Shona\n`sd` - Sindhi\n`si` - Sinhala(Sinhalese)\n`sk` - Slovak\n`sl` - Slovenian\n`so` - Somali\n`es` - Spanish\n`su` - Sundanese\n`sw` - Swahili\n`sv` - Swedish\n");
            embed2.AddField("T", "`tl` - Tagalog(Filipino)\n`tg` - Tajik\n`ta` - Tamil\n`te` - Telugu\n`th` - Thai\n`tr` - Turkish\n");
            embed2.AddField("U", "`uk` - Ukrainian\n`ur` - Urdu\n`uz` - Uzbek\n");
            embed2.AddField("V", "`vi` - Vietnamese\n");
            embed2.AddField("W", "`cy` - Welsh\n\n");
            embed2.AddField("X", "`xh` - Xhosa\n");
            embed2.AddField("Y", "`yi` - Yiddish\n`yo` - Yoruba\n");
            embed2.AddField("Z", "`zu` - Zulu\n");
            await Context.User.SendMessageAsync(string.Empty, false, embed2.Build());
            await ReplyAsync("DM Sent.");
        }

        [Priority(3)]
        [Command("Info")]
        [Summary("Translate Stats for the current guild")]
        public async Task LimitsAsync()
        {
            Limits.Guilds.TryGetValue(Context.Guild.Id, out TranslateLimitsNew.Guild guild);
            if (guild != null)
            {
                await ReplyAsync($"Remaining Characters: {guild.RemainingCharacters()}\n" + $"Total Characters Translated: {guild.TotalCharacters}\n" + $"Translate Limit: {guild.MaxCharacters()}");
                return;
            }

            await ReplyAsync("There is currently no information about this guild stored in the translation service.");
        }
    }
}