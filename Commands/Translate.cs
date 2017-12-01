using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using PassiveBOT.preconditions;
//using Google.Apis.Auth.OAuth2;
//using Google.Cloud.Translation.V2;

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
    }
}