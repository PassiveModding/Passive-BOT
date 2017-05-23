using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ImageSharp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace PassiveBOT.Commands
{
    [Ratelimit(1, 2, Measure.Seconds)]
    public class Images : ModuleBase
    {
        [Command("meme"), Summary("meme"), Alias("memes"), Remarks("Dankness ( ͡° ͜ʖ ͡°)")]
        public async Task Meme()
        {
            int result;
            Random rnd = new Random();
            result = rnd.Next(0, Strings2.meme.Length);

            var builder = new EmbedBuilder()
            {
                ImageUrl = Strings2.meme[result],
                Description = $"Meme Database Size: {Strings2.meme.Length}\n Image Number: {result}"
            };
        
            await ReplyAsync($"", false, builder.Build());

        }

        [Command("dog"), Summary("dog"), Remarks("replies with a cute doggo")]
        public async Task Doggo()
        {
            int result;
            Random rnd = new Random();
            result = rnd.Next(0, Strings3.dog.Length);

            var builder = new EmbedBuilder()
            {
                ImageUrl = Strings3.dog[result],
                Description = $"Out of {Strings3.dog.Length} Doggos\n you got doggo number: {result}"
            };

            await ReplyAsync($"", false, builder.Build());
        }

        [Command("cat"), Summary("cat"), Remarks("replies with a kitty cat")]
        public async Task Kitty()
        {
            int result;
            Random rnd = new Random();
            result = rnd.Next(0, Strings3.cat.Length);

            var builder = new EmbedBuilder()
            {
                ImageUrl = Strings3.cat[result],
                Description = $"Out of {Strings3.cat.Length} Kitties\n you got Kitty number: {result}"
            };

            await ReplyAsync($"", false, builder.Build());
        }

        [Command("salt"), Summary("salt '@user'"), Alias("salty"), Remarks("For salty people")]
        public async Task Salt([Optional] IUser User)
        {
            if (User == null)
            {
                int result;
                Random rnd = new Random();
                result = rnd.Next(0, Strings2.salt.Length);

                var builder = new EmbedBuilder()
                {
                    ImageUrl = Strings2.salt[result],
                    Description = $"Salt Database Size: {Strings2.salt.Length}\n Image Number: {result}"
                };

                await ReplyAsync($"", false, builder.Build());
            }
            else
            {
                int result;
                Random rnd = new Random();
                result = rnd.Next(0, Strings2.salt.Length);

                var builder = new EmbedBuilder()
                {
                    Title = User.Username + " Is a salty ass kid",
                    ImageUrl = Strings2.salt[result],
                    Description = $"Salt Database Size: {Strings2.salt.Length}\n Image Number: {result}"
                };

                await ReplyAsync($"", false, builder.Build());
            }
        }


        [Command("leet"), Summary("leet"), Alias("mlg", "1337", "dank", "doritos", "illuminati", "memz", "pro"), Remarks("MLG  Dankness")]
        public async Task Leet()
        {
            int result;
            Random rnd = new Random();
            result = rnd.Next(0, Strings2.mlg.Length);

            var builder = new EmbedBuilder()
            {
                ImageUrl = Strings2.mlg[result],
                Description = $"MLG Database Size: {Strings2.mlg.Length}\n Image Number: {result}"
            };

            await ReplyAsync($"", false, builder.Build());

        }

        [Command("spoonfed"), Summary("spoonfed"), Remarks("for those who just ask for code")]
        public async Task Spoonfed()
        {
            int result;
            Random rnd = new Random();
            result = rnd.Next(0, Strings.spoon.Length);

            var builder = new EmbedBuilder()
            {
                ImageUrl = Strings.spoon[result]
            };

            await ReplyAsync($"", false, builder.Build());

        }

        [Command("derp"), Summary("derp '@fag'"), Remarks("For special people (note: may be offensive to some)")]
        public async Task Derp([Remainder, Optional]IUser user)
        {
            var title = "";
            int result;
            Random rnd = new Random();
            result = rnd.Next(0, Strings2.autism.Length);
            if (user == null)
                title = $"You suffer from an extreme case of autism";
            else
                title = $"{user.Username} suffers from extreme cases of autism";
            var embed = new EmbedBuilder
            {
                Title = title,
                ImageUrl = $"{Strings2.autism[result]}"
            };
            await ReplyAsync($"", false, embed.Build());
        }
    }
}