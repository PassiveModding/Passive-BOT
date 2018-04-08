using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using PassiveBOT.preconditions;

namespace PassiveBOT.Commands
{
    [Ratelimit(1, 5, Measure.Seconds)]
    public class Translate : ModuleBase
    {
        [Command("translate")]
        [Summary("translate <language-code> <message>")]
        [Remarks("Translate from one language to another")]
        public async Task TranslateCmd(string language, [Remainder] string message)
        {
            var url =
                $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={language}&dt=t&ie=UTF-8&oe=UTF-8&q={Uri.EscapeDataString(message)}";
            var embed = new EmbedBuilder();

            var client = new WebClient {Encoding = Encoding.UTF8};

            var stream = client.OpenRead(url);

            var reader = new StreamReader(stream ?? throw new InvalidOperationException());
            var content = reader.ReadToEnd();
            dynamic file = JsonConvert.DeserializeObject(content);
            embed.AddField($"Original [{file[2]}]", $"{message}");
            embed.AddField($"Final [{language}]", $"{file[0][0][0]}");

            await ReplyAsync("", false, embed.Build());
        }

        [Command("translatedebug")]
        [Summary("translatedebug <language-code> <message>")]
        [Remarks("Translate from one language to another and receive the full info")]
        public async Task DebugTranslate(string language, [Remainder] string message)
        {
            var url =
                $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={language}&dt=t&ie=UTF-8&oe=UTF-8&q={Uri.EscapeDataString(message)}";
            var embed = new EmbedBuilder();

            var client = new WebClient {Encoding = Encoding.UTF8};

            var stream = client.OpenRead(url);

            var reader = new StreamReader(stream ?? throw new InvalidOperationException());
            var content = reader.ReadToEnd();

            await ReplyAsync(content);
            client.Dispose();
        }

        [Command("t list")]
        [Remarks("A list of available languages codes to convert between")]
        [Summary("t list")]
        public async Task Tlist()
        {
            var embed2 = new EmbedBuilder();
            embed2.AddField("INFORMATION", "Format:\n" +
                                           "<Language> <Language Code>\n" +
                                           "Example Usage:\n" +
                                           "`.p translate <language code> <message>`\n" +
                                           "`.p translate es Hi there this will be converted to spanish`");

            /*           string valueString =
                           "Afrikaans af\nAlbanian sq\nAmharic am\nArabic ar\nArmenian hy\nAzeerbaijani az\nBasque eu\nBelarusian be\nBengali bn\nBosnian bs\nBulgarian bg\nCatalan ca\nCebuano ceb\nChinese(Simplified) zh-CN\nChinese(Traditional) zh-TW\nCorsican co\nCroatian hr\nCzech cs\nDanish da\nDutch nl\nEnglish en\nEsperanto eo\nEstonian et\nFinnish fi\nFrench fr\nFrisian fy\nGalician gl\nGeorgian ka\nGerman de\nGreek el\nGujarati gu\nHaitian-Creole ht\nHausa ha\nHawaiian haw\nHebrew iw\nHindi hi\nHmong hmn\nHungarian hu\nIcelandic is \nIgbo ig\nIndonesian id\nIrish ga\nItalian it\nJapanese ja\nJavanese jw\nKannada kn\nKazakh kk\nKhmer km\nKorean ko\nKurdish ku\nKyrgyz ky\nLao lo\nLatin la\nLatvian lv\nLithuanian lt\nLuxembourgish lb\nMacedonian mk\nMalagasy mg\nMalay ms\nMalayalam ml\nMaltese mt\nMaori mi\nMarathi mr\nMongolian mn\nMyanmar(Burmese) my\nNepali ne\nNorwegian no\nNyanja(Chichewa) ny\nPashto ps\nPersian fa\nPolish pl\nPortuguese pt\nPunjabi pa\nRomanian ro\nRussian ru\nSamoan sm\nScots-Gaelic gd\nSerbian sr\nSesotho st\nShona sn\nSindhi sd\nSinhala(Sinhalese) si\nSlovak sk\nSlovenian sl\nSomali so\nSpanish es\nSundanese su\nSwahili sw\nSwedish sv\nTagalog(Filipino) tl\nTajik tg\nTamil ta\nTelugu te\nThai th\nTurkish tr\nUkrainian uk\nUrdu ur\nUzbek uz\nVietnamese vi\nWelsh cy\nXhosa xh\nYiddish yi\nYoruba yo\nZulu zu";
                       string[] values = valueString.Split('\n');
                       var newlist = new List<string>();
                       foreach (var line in values)
                       {
                           string[] v2 = line.Split(' ');
                           string firstword = v2[0];
                           string lastWord = v2[1];
                           v2[0] = $"`{lastWord}` -";
                           v2[1] = firstword;
                           newlist.Add(string.Join(" ", v2));
                       }

                       File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "success.txt"), string.Join("\n", newlist));
                       */
            // "A\n`af` - Afrikaans\n`sq` - Albanian\n`am` - Amharic\n`ar` - Arabic\n`hy` - Armenian\n`az` - Azeerbaijani\nB\n`eu` - Basque\n`be` - Belarusian\n`bn` - Bengali\n`bs` - Bosnian\n`bg` - Bulgarian\nC\n`ca` - Catalan\n`ceb` - Cebuano\n`zh-CN` - Chinese(Simplified)\n`zh-TW` - Chinese(Traditional)\n`co` - Corsican\n`hr` - Croatian\n`cs` - Czech\nD\n`da` - Danish\n`nl` - Dutch\nE\n`en` - English\n`eo` - Esperanto\n`et` - Estonian\nF\n`fi` - Finnish\n`fr` - French\n`fy` - Frisian\nG\n`gl` - Galician\n`ka` - Georgian\n`de` - German\n`el` - Greek\n`gu` - Gujarati\nH\n`ht` - Haitian-Creole\n`ha` - Hausa\n`haw` - Hawaiian\n`iw` - Hebrew\n`hi` - Hindi\n`hmn` - Hmong\n`hu` - Hungarian\nI\n`is` - Icelandic \n`ig` - Igbo\n`id` - Indonesian\n`ga` - Irish\n`it` - Italian\nJ\n`ja` - Japanese\n`jw` - Javanese\nK\n`kn` - Kannada\n`kk` - Kazakh\n`km` - Khmer\n`ko` - Korean\n`ku` - Kurdish\n`ky` - Kyrgyz\nL\n`lo` - Lao\n`la` - Latin\n`lv` - Latvian\n`lt` - Lithuanian\n`lb` - Luxembourgish\nM\n`mk` - Macedonian\n`mg` - Malagasy\n`ms` - Malay\n`ml` - Malayalam\n`mt` - Maltese\n`mi` - Maori\n`mr` - Marathi\n`mn` - Mongolian\n`my` - Myanmar(Burmese)\nN\n`ne` - Nepali\n`no` - Norwegian\n`ny` - Nyanja(Chichewa)\nP\n`ps` - Pashto\n`fa` - Persian\n`pl` - Polish\n`pt` - Portuguese\n`pa` - Punjabi\nR\n`ro` - Romanian\n`ru` - Russian\nS\n`sm` - Samoan\n`gd` - Scots-Gaelic\n`sr` - Serbian\n`st` - Sesotho\n`sn` - Shona\n`sd` - Sindhi\n`si` - Sinhala(Sinhalese)\n`sk` - Slovak\n`sl` - Slovenian\n`so` - Somali\n`es` - Spanish\n`su` - Sundanese\n`sw` - Swahili\n`sv` - Swedish\nT\n`tl` - Tagalog(Filipino)\n`tg` - Tajik\n`ta` - Tamil\n`te` - Telugu\n`th` - Thai\n`tr` - Turkish\nU\n`uk` - Ukrainian\n`ur` - Urdu\n`uz` - Uzbek\nV\n`vi` - Vietnamese\nW\n`cy` - Welsh\n\nx`xh` - Xhosa\nY\n`yi` - Yiddish\n`yo` - Yoruba\nZ\n`zu` - Zulu";
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
            await Context.User.SendMessageAsync("", false, embed2.Build());
            await ReplyAsync("DM Sent.");
        }
    }
}