using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using PassiveBOT.preconditions;
using PassiveBOT.strings;
using PassiveBOT.Services;
using YoutubeExplode;

namespace PassiveBOT.Commands
{
    [Ratelimit(1, 2, Measure.Seconds)]
    public class Media : ModuleBase
    {
        [Command("youtube", RunMode = RunMode.Async)]
        [Summary("youtube 'search'")]
        [Alias("myt")]
        [Remarks("Gives the first youtube result from the given search terms")]
        public async Task Youtube(string search)
        {
            var ytc = new YoutubeClient();
            await ReplyAsync($"Trying to get the top three results for the search: **{search}**\n" +
                             "Please be patient... <3");
            var searchList = await ytc.SearchAsync(search);
            var results = searchList.Take(3);
            var embed = new EmbedBuilder();
            foreach (var item in results)
            {
                var video = await ytc.GetVideoInfoAsync(item);
                embed.AddField(video.Title, $"https://www.youtube.com/watch?v={video.Id}");
            }
            await ReplyAsync("", false, embed.Build());
        }

        [Command("meme")]
        [Summary("meme")]
        [Alias("memes")]
        [Remarks("Dankness ( ͡° ͜ʖ ͡°)")]
        public async Task Meme()
        {
            var str = MemeStr.Meme;
            var rnd = new Random();
            var result = rnd.Next(0, str.Length);

            var builder = new EmbedBuilder()
                .WithImageUrl(str[result])
                .WithFooter(x =>
                {
                    x.WithText($"PassiveBOT | {result}/{str.Length}");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, builder.Build());
        }

        [Command("dog")]
        [Summary("Gets a random dog image from random.dog")]
        public async Task Doggo2()
        {
            var woof = "http://random.dog/" + await SearchHelper.GetResponseStringAsync("https://random.dog/woof")
                           .ConfigureAwait(false);
            var embed = new EmbedBuilder()
                .WithImageUrl(woof)
                .WithTitle("Woof")
                .WithUrl(woof)
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });
            await ReplyAsync("", false, embed.Build());
        }

        [Command("cat")]
        [Summary("Gets a random dog image from random.cat")]
        public async Task Kitty2()
        {
            var xDoc = JsonConvert.DeserializeXNode(await new HttpClient().GetStringAsync("http://random.cat/meow"),
                "root");
            var embed = new EmbedBuilder()
                .WithImageUrl(xDoc.Element("root").Element("file").Value)
                .WithTitle("Meow")
                .WithUrl(xDoc.Element("root").Element("file").Value)
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, embed.Build());
        }

        [Command("salt")]
        [Summary("salt '@user'")]
        [Alias("salty")]
        [Remarks("For salty people")]
        public async Task Salt([Optional] IUser user)
        {
            if (user == null)
            {
                var str = MemeStr.Salt;
                var rnd = new Random();
                var result = rnd.Next(0, str.Length);

                var builder = new EmbedBuilder()
                    .WithImageUrl(str[result])
                    .WithFooter(x =>
                    {
                        x.WithText($"PassiveBOT | {result}/{str.Length}");
                        x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                    });

                await ReplyAsync("", false, builder.Build());
            }
            else
            {
                var str = MemeStr.Salt;
                var rnd = new Random();
                var result = rnd.Next(0, str.Length);

                var builder = new EmbedBuilder()
                    .WithTitle($"{user.Username} is a salty ass kid")
                    .WithImageUrl(str[result])
                    .WithFooter(x =>
                    {
                        x.WithText($"PassiveBOT | {result}/{str.Length}");
                        x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                    });

                await ReplyAsync("", false, builder.Build());
            }
        }

        [Command("leet")]
        [Summary("leet")]
        [Alias("mlg", "1337", "dank", "doritos", "illuminati", "memz", "pro")]
        [Remarks("MLG  Dankness")]
        public async Task Leet()
        {
            var str = MemeStr.Mlg;
            var rnd = new Random();
            var result = rnd.Next(0, str.Length);

            var builder = new EmbedBuilder()
                .WithImageUrl(str[result])
                .WithFooter(x =>
                {
                    x.WithText($"PassiveBOT | {result}/{str.Length}");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, builder.Build());
        }

        [Command("spoonfed")]
        [Summary("spoonfed")]
        [Remarks("for those who just ask for code")]
        public async Task Spoonfed()
        {
            var str = MemeStr.Spoon;
            var rnd = new Random();
            var result = rnd.Next(0, str.Length);

            var builder = new EmbedBuilder()
                .WithImageUrl(str[result])
                .WithFooter(x =>
                {
                    x.WithText($"PassiveBOT | {result}/{str.Length}");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, builder.Build());
        }

        [Command("derp")]
        [Summary("derp '@fag'")]
        [Remarks("For special people (note: may be offensive to some)")]
        public async Task Derp([Remainder] [Optional] IUser user)
        {
            var title = user == null
                ? "You suffer from an extreme case of autism"
                : $"{user.Username} suffers from extreme cases of autism";

            var str = MemeStr.Autism;
            var rnd = new Random();
            var result = rnd.Next(0, str.Length);

            var builder = new EmbedBuilder()
                .WithTitle($"{title}")
                .WithImageUrl(str[result])
                .WithFooter(x =>
                {
                    x.WithText($"PassiveBOT | {result}/{str.Length}");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, builder.Build());
        }
    }
}