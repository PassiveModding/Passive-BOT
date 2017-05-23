using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PassiveBOT.Commands
{
    [Ratelimit(1, 2, Measure.Seconds), NSFWchat]
    public class NSFW : ModuleBase
    {
        [Command("tits"), Summary("tits"), Alias("boobs", "rack"), Remarks("Fetches some sexy titties")]
        public async Task BoobsAsync()
        {
                JToken obj;
                var rnd = new Random().Next(0, 10229);
                using (var http = new HttpClient())
                    obj = JArray.Parse(await http.GetStringAsync($"http://api.oboobs.ru/boobs/{rnd}"))[0];
                var builder = new EmbedBuilder()
                {
                    ImageUrl = $"http://media.oboobs.ru/{obj["preview"].ToString()}",
                    Description = $"Tits Database Size: 10229\n Image Number: {rnd}"
                };
                await ReplyAsync($"", false, builder.Build());
        }
        [Command("Ass"), Summary("ass"), Remarks("Sexy Ass!")]
        public async Task BumsAsync()
        {
                JToken obj;
                var rnd = new Random().Next(0, 4222);
                using (var http = new HttpClient())
                    obj = JArray.Parse(await http.GetStringAsync($"http://api.obutts.ru/butts/{rnd}"))[0];
                var builder = new EmbedBuilder()
                {
                    ImageUrl = $"http://media.obutts.ru/{obj["preview"].ToString()}",
                    Description = $"Ass Database Size: 4222\n Image Number: {rnd}"
                };
                await ReplyAsync($"", false, builder.Build());
        }
        [Command("nsfw"), Summary("nsfw"), Alias("nude", "porn"), Remarks("Sexy Stuff!")]
        public async Task Porn()
        {
            int result;
            Random rnd = new Random();
            result = rnd.Next(0, Strings.nsfw.Length);

            var builder = new EmbedBuilder()
            {
                ImageUrl = Strings.nsfw[result],
                Description = $"NSFW Database Size: {Strings.nsfw.Length}\n Image Number: {result}"
            };
            await ReplyAsync($"", false, builder.Build());
        }
        [Command("sfw"), Summary("sfw"), Remarks("Porn meets MS Paint")]
        public async Task Sfw()
        {
            int result;
            Random rnd = new Random();
            result = rnd.Next(0, Strings2.sfw.Length);

            var builder = new EmbedBuilder()
            {
                ImageUrl = Strings2.sfw[result],
                Description = $"SFW Database Size: {Strings2.sfw.Length}\n Image Number: {result}"
            };
            await ReplyAsync($"", false, builder.Build());
        }
        [Command("nsfwvid"), Summary("nsfwvid"), Remarks("Porhub + Bots = win?")]
        public async Task Nsfwvid()
        {
            int result;
            Random rnd = new Random();
            result = rnd.Next(0, Strings2.sfw.Length);
            var builder = new EmbedBuilder()
            {
                Title = Strings.nsfwvid[result],
                Url = Strings.nsfwvid[result],
                Description = $"Video Database Size: {Strings.nsfwvid.Length}\n Video Number: {result}"
            };
            await ReplyAsync($"", false, builder.Build());
        }
    }
}