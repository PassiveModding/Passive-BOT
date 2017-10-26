using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;
using Newtonsoft.Json;
using PassiveBOT.Configuration;
using PassiveBOT.preconditions;

namespace PassiveBOT.Commands
{
    [Ratelimit(1, 5, Measure.Seconds)]
    public class Translate : ModuleBase
    {
        [Command("freetranslate")]
        [Summary("freetranslate <language-code> <message>")]
        [Remarks("Translate from one language to another")]
        public async Task TranslateCmd(string language, [Remainder] string message)
        {
            var languages = new List<string> {"fr", "en", "es", "tl", "pt"};
            if (!languages.Contains(language))
            {
                await ReplyAsync("Unsupported Language");
                var embed2 = new EmbedBuilder();
                embed2.AddField("Languages",
                    "`fr` - french(français)\n" +
                    "`en` - english\n" +
                    "`es` - spanish(Español)\n" +
                    "`tl` - filipino\n" +
                    "`pt` - portugese (português)");
                await ReplyAsync("", false, embed2.Build());
                return;
            }
            var url = "https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl=" + language +
                      "&dt=t&q=" + Uri.EscapeDataString(message);
            var embed = new EmbedBuilder();

            using (var client = new WebClient())
            {
                client.Encoding = Encoding.Unicode;
                client.DownloadFile($"{url}", $"{Context.Message.Id}.txt");
            }

            dynamic file =
                JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,
                    $"{Context.Message.Id}.txt")));

            embed.AddField($"Original [{file[2]}]", $"{message}");
            embed.AddField($"Final [{language}]", $"{file[0][0][0]}");


            await ReplyAsync("", false, embed.Build());
            File.Delete(Path.Combine(AppContext.BaseDirectory, $"{Context.Message.Id}.txt"));
        }

        [Command("premium")]
        [Summary("premium <serial>")]
        [Remarks("set the current server up for Premium discord translate commands.")]
        public async Task Premium([Remainder] string key = null)
        {
            if (key == null)
            {
                await ReplyAsync(
                    "Please upgrade to use the premium version of the Translation API to use the premium translation commands, and convert between 80+ languages with ease\n" +
                    "Purchase: http://rocketr.net/sellers/passivemodding");
                return;
            }
            if (Program.Keys.Contains(key))
            {
                var g = GuildConfig.GetServer(Context.Guild);
                if (g.Premium)
                {
                    await ReplyAsync("Server is already premium. Save this key for later!");
                    return;
                }
                Program.Keys.Remove(key);
                g.Premium = true;
                g.TimeOffset = DateTime.UtcNow.AddDays(28);
                GuildConfig.SaveServer(g, Context.Guild);

                await ReplyAsync("SUCCESS! Server status is now premium. Enjoy Full Translation access!!!\n" +
                                 "Limits are 100k characters in 28 days. Otherwise ENJOY!!");
            }
            else
            {
                await ReplyAsync(
                    "Invalid Key! Please upgrade to use the premium version of the Translation API to use the premium translation commands, and convert between 80+ languages with ease\n" +
                    "Purchase: http://rocketr.net/sellers/passivemodding");
            }
        }

        [Command("translate")]
        [Summary("translate <target language code> <message>")]
        [Remarks("Translate from one language to another")]
        public async Task Translate2(string target, [Remainder] string message)
        {
            if (message.Length > 100)
            {
                await ReplyAsync($"Message length too long. {message.Length}/100");
                return;
            }
            if (!GuildConfig.Load(Context.Guild.Id).Premium)
            {
                await ReplyAsync(
                    "Please upgrade to use the premium version of the Translation API to use this command, and convert between 80+ languages with ease\n" +
                    "Purchase: http://rocketr.net/sellers/passivemodding");
                return;
            }

            if (GuildConfig.Load(Context.Guild.Id).TimeOffset < DateTime.UtcNow &&
                GuildConfig.Load(Context.Guild.Id).Premium)
            {
                var g = GuildConfig.GetServer(Context.Guild);
                g.Premium = false;
                g.Characters = 0;
                await ReplyAsync(
                    "Premium time has ended for the server, you may purchase a new on here: http://rocketr.net/sellers/passivemodding");
                GuildConfig.SaveServer(g, Context.Guild);
                return;
            }
            if (GuildConfig.Load(Context.Guild.Id).Characters > 100000)
            {
                var g = GuildConfig.GetServer(Context.Guild);
                g.Premium = false;
                g.Characters = 0;
                await ReplyAsync(
                    "Premium time has ended for the server, you may purchase a new on here: http://rocketr.net/sellers/passivemodding");
                GuildConfig.SaveServer(g, Context.Guild);
                return;
            }


            try
            {
                var client =
                    TranslationClient.Create(GoogleCredential.FromFile(@"D:\OneAll-Site-556165-2-3782d4d4869a.json"));
                var response = client.TranslateText($"{message}", $"{target}");
                var embed = new EmbedBuilder();
                embed.AddField($"Original [{response.DetectedSourceLanguage}]", message);
                embed.AddField($"Translated [{response.TargetLanguage}]", response.TranslatedText);
                embed.Color = Color.Green;
                var g = GuildConfig.GetServer(Context.Guild);
                g.Characters = g.Characters + message.Length;
                embed.WithFooter(x =>
                {
                    x.Text =
                        $"{GuildConfig.Load(Context.Guild.Id).TimeOffset - DateTime.UtcNow:dd\\ \\D\\a\\y\\s\\ hh\\ \\H\\o\\u\\r\\s} Left || [{g.Characters}/100000]";
                });
                GuildConfig.SaveServer(g, Context.Guild);
                await ReplyAsync("", false, embed.Build());
            }
            catch
            {
                await ReplyAsync("Invalid Language code! Please use one on the list using the command `.p t-list`");
            }
        }

        [Command("t-list")]
        [Remarks("A list of available languages codes to convert between for premium")]
        [Summary("t-list")]
        public async Task Tlist()
        {
            var embed2 = new EmbedBuilder();
            embed2.AddField("INFORMATION", "Format:\n" +
                                           "<Language> <Language Code>\n" +
                                           "Example Usage:\n" +
                                           "`.p translate <language code> <message>`\n" +
                                           "`.p translate es Hi there this will be converted to spanish`");
            embed2.AddField("A",
                "Afrikaans af\n" +
                "Albanian sq\n" +
                "Amharic  am\n" +
                "Arabic   ar\n" +
                "Armenian hy\n" +
                "Azeerbaijani az\n");
            embed2.AddField("B",
                "Basque   eu\n" +
                "Belarusian   be\n" +
                "Bengali  bn\n" +
                "Bosnian  bs\n" +
                "Bulgarian    bg\n");
            embed2.AddField("C",
                "Catalan  ca\n" +
                "Cebuano  ceb\n" +
                "Chinese(Simplified)    zh-CN\n" +
                "Chinese(Traditional) zh-TW\n" +
                "Corsican  co\n" +
                "Croatian hr\n" +
                "Czech    cs\n");
            embed2.AddField("D",
                "Danish   da" +
                "Dutch    nl\n");
            embed2.AddField("E",
                "English  en\n" +
                "Esperanto    eo\n" +
                "Estonian et\n");
            embed2.AddField("f",
                "Finnish  fi\n" +
                "French   fr\n" +
                "Frisian  fy\n");
            embed2.AddField("g",
                "Galician gl\n" +
                "Georgian ka\n" +
                "German   de\n" +
                "Greek    el\n" +
                "Gujarati gu\n");
            embed2.AddField("h",
                "Haitian Creole   ht\n" +
                "Hausa    ha\n" +
                "Hawaiian haw\n" +
                "Hebrew  iw\n" +
                "Hindi    hi\n" +
                "Hmong    hmn\n" +
                "Hungarian   hu\n");
            embed2.AddField("i",
                "Icelandic is \n" +
                "Igbo ig\n" +
                "Indonesian   id\n" +
                "Irish    ga\n" +
                "Italian  it\n"
            );
            embed2.AddField("j",
                "Japanese ja\n" +
                "Javanese jw\n");
            embed2.AddField("k",
                "Kannada  kn\n" +
                "Kazakh   kk\n" +
                "Khmer    km\n" +
                "Korean   ko\n" +
                "Kurdish  ku\n" +
                "Kyrgyz   ky\n");
            embed2.AddField("l",
                "Lao  lo\n" +
                "Latin    la\n" +
                "Latvian  lv\n" +
                "Lithuanian   lt\n" +
                "Luxembourgish    lb\n");
            embed2.AddField("m",
                "Macedonian   mk\n" +
                "Malagasy mg\n" +
                "Malay    ms\n" +
                "Malayalam    ml" +
                "Maltese  mt\n" +
                "Maori    mi\n" +
                "Marathi  mr\n" +
                "Mongolian    mn\n" +
                "Myanmar(Burmese)    my\n");
            embed2.AddField("n",
                "Nepali   ne\n" +
                "Norwegian    no\n" +
                "Nyanja(Chichewa)    ny\n");
            embed2.AddField("p",
                "Pashto   ps\n" +
                "Persian  fa\n" +
                "Polish   pl\n" +
                "Portuguese   pt\n" +
                "Punjabi  pa\n");
            embed2.AddField("r",
                "Romanian ro\n" +
                "Russian  ru\n");
            embed2.AddField("s",
                "Samoan   sm\n" +
                "Scots Gaelic gd\n" +
                "Serbian  sr\n" +
                "Sesotho  st\n" +
                "Shona    sn\n" +
                "Sindhi   sd" +
                "Sinhala(Sinhalese)  si\n" +
                "Slovak   sk\n" + "Slovenian    sl\n" +
                "Somali   so\n" +
                "Spanish  es\n" +
                "Sundanese    su\n" +
                "Swahili  sw\n" +
                "Swedish  sv\n");
            embed2.AddField("t",
                "Tagalog(Filipino)   tl\n" +
                "Tajik    tg" +
                "Tamil    ta\n" +
                "Telugu   te\n" +
                "Thai th\n" +
                "Turkish  tr\n");
            embed2.AddField("u",
                "Ukrainian    uk\n" +
                "Urdu ur\n" +
                "Uzbek    uz\n");
            embed2.AddField("v",
                "Vietnamese   vi\n");
            embed2.AddField("w",
                "Welsh    cy\n");
            embed2.AddField("x",
                "Xhosa    xh\n");
            embed2.AddField("y",
                "Yiddish  yi\n" +
                "Yoruba   yo");
            embed2.AddField("z",
                "Zulu zu\n");
            await Context.User.SendMessageAsync("", false, embed2.Build());
            await ReplyAsync("DM Sent.");
        }
    }
}