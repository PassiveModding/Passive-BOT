using System;
using System.Data;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;
using PassiveBOT.Configuration.Objects;
using PassiveBOT.preconditions;

namespace PassiveBOT.Commands.Text
{
    [Ratelimit(1, 10, Measure.Seconds)]
    public class Chat : ModuleBase
    {
        [Command("say")]
        [Summary("say <message>")]
        [Alias("echo")]
        [Remarks("Echos the provided input")]
        public async Task Say([Remainder] string input)
        {
            input = input.Replace("@everyone", "Everyone");
            input = input.Replace("@here", "Here");
            await ReplyAsync("\u200B" + input);
        }

        [Command("calculate")]
        [Summary("calculate <math expression>")]
        [Remarks("calculates the given input. use brackets where possible in complex calculations.")]
        public async Task Calculate([Remainder] string input)
        {
            input = input.Replace(" ", "").ToLower();
            var working = $"{input}\n";

            //Replace special integers
            if (input.Contains(")("))
            {
                input = input.Replace(")(", ")*(");
                working += $"{input}\n";
            }

            foreach (var section in Regex.Matches(input, @"\([\S\s]+?\)"))
            {
                var sec = section.ToString().Replace("(", "").Replace(")", "");
                //Replace special integers
                if (sec.Contains("e"))
                {
                    sec = Regex.Replace(sec, @"([0-9])e", $"$1*{Math.E}");
                    sec = Regex.Replace(sec, @"e([0-9])", $"{Math.E}*$1");
                    sec = sec.Replace("e", $"{Math.E}");
                    working += $"simplify: {section} => {sec}\n";
                }

                var sec1 = sec;
                //Fix Powers
                if (sec.Contains("^"))
                {
                    var matches = Regex.Matches(sec, @"[-+]?([0-9]*\.[0-9]+|[0-9]+)\^[-+]?([0-9]*\.[0-9]+|[0-9]+)");
                    foreach (var match in matches)
                    {
                        var intlist = Regex.Matches(match.ToString(), @"[-+]?([0-9]*\.[0-9]+|[0-9]+)");
                        var intbase = double.Parse(intlist[0].ToString());
                        var intpow = double.Parse(intlist[1].ToString());
                        var sum = Math.Pow(intbase, intpow);
                        sec = sec.Replace(match.ToString(), sum.ToString(CultureInfo.InvariantCulture));
                    }

                    working = $"simplify: {sec1} => {sec}\n";
                }

                sec = new DataTable().Compute(sec, null).ToString();
                working += $"{section} => {sec}\n";
                input = input.Replace(section.ToString(), sec);
                working += $"{input}\n";
            }

            //Replace special integers
            if (input.Contains("e"))
            {
                input = Regex.Replace(input, @"([0-9])e", $"$1*{Math.E}");
                input = Regex.Replace(input, @"e([0-9])", $"{Math.E}*$1");
                input = input.Replace("e", $"{Math.E}");
                working += $"{input}\n";
            }


            //Fix Powers
            if (input.Contains("^"))
            {
                var matches = Regex.Matches(input, @"[-+]?([0-9]*\.[0-9]+|[0-9]+)\^[-+]?([0-9]*\.[0-9]+|[0-9]+)");
                foreach (var match in matches)
                {
                    var intlist = Regex.Matches(match.ToString(), @"[-+]?([0-9]*\.[0-9]+|[0-9]+)");
                    var intbase = double.Parse(intlist[0].ToString());
                    var intpow = double.Parse(intlist[1].ToString());
                    var sum = Math.Pow(intbase, intpow);
                    input = input.Replace(match.ToString(), sum.ToString(CultureInfo.InvariantCulture));
                }

                working += $"{input}\n";
            }

            var output = new DataTable().Compute(input, null).ToString();

            await ReplyAsync($"__**Working**__\n" +
                             $@"{working.Replace("*", @"\*")}" +
                             $"\n" +
                             $"__**Final**__\n" +
                             $@"{Regex.Escape(input)} = {output}");
        }

        [Command("quote")]
        [Summary("quote <msg ID>")]
        [Remarks("Quotes the given message (from message ID)")]
        public async Task Quote([Remainder] ulong id)
        {
            var msg = await Context.Channel.GetMessageAsync(id);

            if (msg == null)
            {
                await ReplyAsync("This message is unavailable");
                return;
            }

            var embed = new EmbedBuilder
            {
                Title = msg.Author.Username,
                Description = msg.ToString(),
                Timestamp = msg.Timestamp
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("dm")]
        [Summary("dm <@user> <message>")]
        [Remarks("Direct Messages the specified user")]
        public async Task Dmuser([Optional] IUser recipient, [Remainder] string message)
        {
            if (recipient == null)
            {
                var dm = await Context.User.GetOrCreateDMChannelAsync();
                await dm.SendMessageAsync(message);
                await ReplyAsync("check your DMs babe :heart:");
            }
            else
            {
                var dm = await recipient.GetOrCreateDMChannelAsync();
                await dm.SendMessageAsync($"Message from {Context.User}: {message}");
                await ReplyAsync($"check your DMs babe :heart: {recipient.Mention}");
            }
        }

        [Command("hug")]
        [Summary("hug <@user>")]
        [Remarks("Gives a big sloppy hug")]
        public async Task Huguser(IUser user)
        {
            await ReplyAsync($"Hey {user.Mention}, {Context.User.Mention} sent you a big warm hug :heart:");
        }

        [Command("clapclap")]
        [Summary("clapclap <message>")]
        [Remarks("replace all spaces with claps")]
        public async Task ClapClap([Remainder] string message)
        {
            await ReplyAsync(message.Replace(" ", ":clap:"));
        }

        [Command("date")]
        [Summary("date <@user>")]
        [Remarks("Asks them out on a date")]
        public async Task Date(IUser user, [Remainder] string input)
        {
            await ReplyAsync(
                $"Hey Cutie {user.Mention}, {Context.User.Mention} really likes you and would \nlove to go on a date with you, do you accept? :heart:\n" +
                $"Heres a cute love message from them ```\n{input}\n```");
        }
    }
}