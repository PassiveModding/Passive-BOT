using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using PassiveBOT.preconditions;
using PassiveBOT.strings;

namespace PassiveBOT.Commands
{
    [Ratelimit(1, 2, Measure.Seconds)]
    [CheckNsfw]
    public class Nsfw : ModuleBase
    {
        [Command("tits")]
        [Summary("tits")]
        [Alias("boobs", "rack")]
        [Remarks("Fetches some sexy titties")]
        public async Task BoobsAsync()
        {
            JToken obj;
            var rnd = new Random().Next(0, 10229);
            using (var http = new HttpClient())
            {
                obj = JArray.Parse(await http.GetStringAsync($"http://api.oboobs.ru/boobs/{rnd}"))[0];
            }
            var builder = new EmbedBuilder
            {
                ImageUrl = $"http://media.oboobs.ru/{obj["preview"]}",
                Description = $"Tits Database Size: 10229\n Image Number: {rnd}",
                Title = "Tits",
                Url = $"http://adult.passivenation.com/18217229/http://media.oboobs.ru/{obj["preview"]}"
            };


            await ReplyAsync("", false, builder.Build());
        }

        [Command("Ass")]
        [Summary("ass")]
        [Remarks("Sexy Ass!")]
        public async Task BumsAsync()
        {
            JToken obj;
            var rnd = new Random().Next(0, 4222);
            using (var http = new HttpClient())
            {
                obj = JArray.Parse(await http.GetStringAsync($"http://api.obutts.ru/butts/{rnd}"))[0];
            }
            var builder = new EmbedBuilder
            {
                ImageUrl = $"http://media.obutts.ru/{obj["preview"]}",
                Description = $"Ass Database Size: 4222\n Image Number: {rnd}",
                Title = "Ass",
                Url = $"http://adult.passivenation.com/18217229/http://media.obutts.ru/{obj["preview"]}/"
            };
            await ReplyAsync("", false, builder.Build());
        }

        [Command("nsfw")]
        [Summary("nsfw")]
        [Alias("nude", "porn")]
        [Remarks("Sexy Stuff!")]
        public async Task Porn()
        {
            var str = NsfwStr.Nsfw;
            var rnd = new Random();
            var result = rnd.Next(0, str.Length);

            var builder = new EmbedBuilder()
                .WithTitle("NSFW")
                .WithUrl($"http://adult.passivenation.com/18217229/{str[result]}/")
                .WithImageUrl(str[result])
                .WithFooter(x =>
                {
                    x.WithText($"PassiveBOT | {result}/{str.Length}");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, builder.Build());
        }

        [Command("sfw")]
        [Summary("sfw")]
        [Remarks("Porn meets MS Paint")]
        public async Task Sfw()
        {
            var str = NsfwStr.Sfw;
            var rnd = new Random();
            var result = rnd.Next(0, str.Length);

            var builder = new EmbedBuilder()
                .WithTitle("SFW")
                .WithUrl($"http://adult.passivenation.com/18217229/{str[result]}/")
                .WithImageUrl(str[result])
                .WithFooter(x =>
                {
                    x.WithText($"PassiveBOT | {result}/{str.Length}");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, builder.Build());
        }

        [Command("nsfwvid")]
        [Summary("nsfwvid")]
        [Remarks("Pornhub + Bots = win?")]
        public async Task Nsfwvid()
        {
            var str = NsfwStr.Nsfwvid;
            var rnd = new Random();
            var result = rnd.Next(0, str.Length);

            var builder = new EmbedBuilder()
                .WithTitle("Click for Random Video")
                .WithUrl("http://adult.passivenation.com/18217229/{str[result]}/")
                .WithFooter(x =>
                {
                    x.WithText($"PassiveBOT | {result}/{str.Length}");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, builder.Build());
        }

        [Command("pussy")]
        [Summary("pussy")]
        [Remarks(";)")]
        public async Task Pussy()
        {
            var str = NsfwStr.Pussy;
            var rnd = new Random();
            var result = rnd.Next(0, str.Length);

            var builder = new EmbedBuilder()
                .WithTitle("Pussy")
                .WithUrl($"http://adult.passivenation.com/18217229/{str[result]}/")
                .WithFooter(x =>
                {
                    x.WithText($"PassiveBOT | {result}/{str.Length}");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, builder.Build());
            await ReplyAsync(str[result]);
        }

        [Command("nsfwgif")]
        [Summary("nsfwgif")]
        [Remarks("Gifs")]
        public async Task Ngif()
        {
            var str = NsfwStr.Nsfwgif;
            var rnd = new Random();
            var result = rnd.Next(0, str.Length);

            var builder = new EmbedBuilder()
                .WithTitle("NSFW GIF")
                .WithUrl($"http://adult.passivenation.com/18217229/{str[result]}/")
                .WithFooter(x =>
                {
                    x.WithText($"PassiveBOT | {result}/{str.Length}");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, builder.Build());

            await ReplyAsync(str[result]);
        }
    }
}