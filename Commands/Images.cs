using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.preconditions;
using PassiveBOT.strings;

namespace PassiveBOT.Commands
{
    [Ratelimit(1, 2, Measure.Seconds)]
    public class Images : ModuleBase
    {
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
        [Summary("dog")]
        [Remarks("replies with a cute doggo")]
        public async Task Doggo()
        {
            var str = AnimalStr.Dog;
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

        [Command("cat")]
        [Summary("cat")]
        [Remarks("replies with a kitty cat")]
        public async Task Kitty()
        {
            var str = AnimalStr.Cat;
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
            var title = user == null ? "You suffer from an extreme case of autism" : $"{user.Username} suffers from extreme cases of autism";

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