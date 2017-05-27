using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using Discord.WebSocket;
using ImageSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace PassiveBOT.Commands
{
    [Ratelimit(1, 10, Measure.Seconds)]
    public class Chat : ModuleBase
    {
        [Command("say"), Summary("say 'hi'"), Alias("echo"), Remarks("Echos the provided input")]
        public async Task Say([Remainder] string input)
        {
            if (input.Contains("@everyone") || input.Contains("@here"))
            {
                await ReplyAsync("\u200B Fight me you little scrub");
            }
            else
                await ReplyAsync("\u200B" + input);
        }
        [Command("quote"), Summary("quote 'messageID'"), Remarks("Quotes the given message (from message ID)")]
        public async Task Quote([Remainder]ulong id)
        {
            var msg = await Context.Channel.GetMessageAsync(id);
            var user = msg.Author;
            var thumb = msg.Author.GetAvatarUrl();
            var time = msg.Timestamp;

            var embed = new EmbedBuilder()
            {
                Title = user.Username.ToString(),
                Description = msg.ToString(),
                Timestamp = time,
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("dm"), Summary("dm @sexyperson 'hey babe'"), Remarks("Direct Messages the specified user")]
        public async Task Dmuser([Optional]IUser recipient, [Remainder] string message)
        {
            if (recipient == null)
            {
                var dm = await Context.User.CreateDMChannelAsync();
                await dm.SendMessageAsync(message);
                await ReplyAsync("check your DMs babe :heart:");
            }
            else
            {
                var dm = await recipient.CreateDMChannelAsync();
                await dm.SendMessageAsync($"Message from {Context.User}: {message}");
                await ReplyAsync($"check your DMs babe :heart: {recipient.Mention}");
            }
        }

        [Command("hug"), Summary("hug @bae"), Remarks("Gives a big sloppy hug")]
        public async Task Huguser(IUser user)
        {
            await ReplyAsync($"Hey {user.Mention}, {Context.User.Mention} sent you a big warm hug :heart:");
        }

        [Command("date"), Summary("date @babe"), Remarks("Asks them out on a date")]
        public async Task Date(IUser user, [Remainder]string input)
        {
            await ReplyAsync($"Hey Cutie {user.Mention}, {Context.User.Mention} really likes you and would \nlove to go on a date with you, do you accept? :heart:\n" +
                $"Heres a cute love message from them ```\n{input}\n```");
        }

        [Command("revivechat"), Summary("revivechat"), Alias("revive", "wakeup"), Remarks("wakes everyone up (once every hour max)"), Ratelimit(1, 60, Measure.Minutes)]
        public async Task Revive()
            =>await ReplyAsync("pay attention my bitches, theres chatting to do @everyone");
    }
}