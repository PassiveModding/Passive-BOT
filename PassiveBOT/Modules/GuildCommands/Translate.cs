// ReSharper disable StringLiteralTypo
namespace PassiveBOT.Modules.GuildCommands
{
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.Commands;

    using PassiveBOT.Discord.Context;
    using PassiveBOT.Discord.Extensions;
    using PassiveBOT.Models;

    /// <summary>
    /// The translate commands
    /// </summary>
    [Group("Translate")]
    public class Translate : Base
    {
        /// <summary>
        /// The translate cmd.
        /// </summary>
        /// <param name="languageCode">
        /// The language code.
        /// </param>
        /// <param name="message">
        /// The message to translate.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command]
        [Summary("Translate from one language to another")]
        public async Task TranslateCmd(LanguageMap.LanguageCode languageCode, [Remainder] string message)
        {
            var embed = new EmbedBuilder { Title = "Translate", Color = Color.Blue };
            var original = TextManagement.FixLength(message);
            var language = TranslateMethods.LanguageCodeToString(languageCode);
            var file = TranslateMethods.TranslateMessage(language, message);
            var response = TextManagement.FixLength(TranslateMethods.HandleResponse(file));
            embed.AddField($"Translated [{language} || {language}]", $"{response}", true);
            embed.AddField($"Original [{file[2]}]", $"{original}", true);

            await ReplyAsync(string.Empty, false, embed.Build());
        }

        /// <summary>
        /// lists translation languages
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("languages")]
        [Summary("A list of available languages codes to convert between")]
        public async Task TranslateList()
        {
            var embed2 = new EmbedBuilder();
            embed2.AddField("INFORMATION", "Format:\n" +
                                           "<Language> <Language Code>\n" +
                                           "Example Usage:\n" +
                                           "`.p translate <language code> <message>`\n" +
                                           "`.p translate es Hi there this will be converted to spanish`");
            embed2.AddField("A",
                "`af` - Afrikaans\n`sq` - Albanian\n`am` - Amharic\n`ar` - Arabic\n`hy` - Armenian\n`az` - Azeerbaijani\n");
            embed2.AddField("B",
                "`eu` - Basque\n`be` - Belarusian\n`bn` - Bengali\n`bs` - Bosnian\n`bg` - Bulgarian\n");
            embed2.AddField("C",
                "`ca` - Catalan\n`ceb` - Cebuano\n`zh-CN` - Chinese(Simplified)\n`zh-TW` - Chinese(Traditional)\n`co` - Corsican\n`hr` - Croatian\n`cs` - Czech\n");
            embed2.AddField("D",
                "`da` - Danish\n`nl` - Dutch\n");
            embed2.AddField("E",
                "`en` - English\n`eo` - Esperanto\n`et` - Estonian\n");
            embed2.AddField("f",
                "`fi` - Finnish\n`fr` - French\n`fy` - Frisian\n");
            embed2.AddField("g",
                "`gl` - Galician\n`ka` - Georgian\n`de` - German\n`el` - Greek\n`gu` - Gujarati\n");
            embed2.AddField("h",
                "`ht` - Haitian-Creole\n`ha` - Hausa\n`haw` - Hawaiian\n`iw` - Hebrew\n`hi` - Hindi\n`hmn` - Hmong\n`hu` - Hungarian\n");
            embed2.AddField("i",
                "`is` - Icelandic \n`ig` - Igbo\n`id` - Indonesian\n`ga` - Irish\n`it` - Italian\n");
            embed2.AddField("j",
                "`ja` - Japanese\n`jw` - Javanese\n");
            embed2.AddField("k",
                "`kn` - Kannada\n`kk` - Kazakh\n`km` - Khmer\n`ko` - Korean\n`ku` - Kurdish\n`ky` - Kyrgyz\n");
            embed2.AddField("l",
                "`lo` - Lao\n`la` - Latin\n`lv` - Latvian\n`lt` - Lithuanian\n`lb` - Luxembourgish\n");
            embed2.AddField("m",
                "`mk` - Macedonian\n`mg` - Malagasy\n`ms` - Malay\n`ml` - Malayalam\n`mt` - Maltese\n`mi` - Maori\n`mr` - Marathi\n`mn` - Mongolian\n`my` - Myanmar(Burmese)\n");
            embed2.AddField("n",
                "`ne` - Nepali\n`no` - Norwegian\n`ny` - Nyanja(Chichewa)\n");
            embed2.AddField("p",
                "`ps` - Pashto\n`fa` - Persian\n`pl` - Polish\n`pt` - Portuguese\n`pa` - Punjabi\n");
            embed2.AddField("r",
                "`ro` - Romanian\n`ru` - Russian\n");
            embed2.AddField("s",
                "`sm` - Samoan\n`gd` - Scots-Gaelic\n`sr` - Serbian\n`st` - Sesotho\n`sn` - Shona\n`sd` - Sindhi\n`si` - Sinhala(Sinhalese)\n`sk` - Slovak\n`sl` - Slovenian\n`so` - Somali\n`es` - Spanish\n`su` - Sundanese\n`sw` - Swahili\n`sv` - Swedish\n");
            embed2.AddField("t",
                "`tl` - Tagalog(Filipino)\n`tg` - Tajik\n`ta` - Tamil\n`te` - Telugu\n`th` - Thai\n`tr` - Turkish\n");
            embed2.AddField("u",
                "`uk` - Ukrainian\n`ur` - Urdu\n`uz` - Uzbek\n");
            embed2.AddField("v",
                "`vi` - Vietnamese\n");
            embed2.AddField("w",
                "`cy` - Welsh\n\n");
            embed2.AddField("x",
                "`xh` - Xhosa\n");
            embed2.AddField("y",
                "`yi` - Yiddish\n`yo` - Yoruba\n");
            embed2.AddField("z",
                "`zu` - Zulu\n");
            await Context.User.SendMessageAsync(string.Empty, false, embed2.Build());
            await ReplyAsync("DM Sent.");
        }
    }
}