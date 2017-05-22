using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ImageSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace PassiveBOT.Commands
{
    [Ratelimit(1, 10, Measure.Seconds)]
    public class Chat : ModuleBase
    {
        [Command("say"), Summary("say 'hi'"), Alias("echo"), Remarks("Echos the provided input")]
        public async Task Say([Remainder] string input)
            =>await ReplyAsync("\u200B" + input);

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

        [Command("react"), Summary("react :ok_hand: :wink:"), Remarks("reacts the given emoji(s)")]
        public async Task React([Remainder]string emoji)
        {
            var message = Context.Message;
            new string(emoji.ToCharArray().Distinct().ToArray());

            string[] split = emoji.Split(' ');

            var e = new string(emoji.ToCharArray().Distinct().ToArray());
            string[] emoj = e.Split(' ');

            foreach (string emojiii in emoj)
            {
                await Task.Delay(260);
                await message.AddReactionAsync($"{emojiii}");
            }
        }

        [Command("reactID"), Summary("react 313287245429866506 :ok_hand: _wink:"), Remarks("reacts the given emoji(s) to a message (from ID)")]
        public async Task ReactID(ulong id, [Remainder]string emoji)
        {
            var message = await Context.Channel.GetMessageAsync(id) as IUserMessage;

            new string(emoji.ToCharArray().Distinct().ToArray());
            string[] split = emoji.Split(' ');

            var e = new string(emoji.ToCharArray().Distinct().ToArray());
            string[] emoj = e.Split(' ');

            foreach (string emojiii in emoj)
            {
                await Task.Delay(260);
                await message.AddReactionAsync($"{emojiii}");
            }
        }

        [Command("donate"), Summary("donate"), Alias("support"), Remarks("Donation Links for PassiveModding")]
        public async Task Donate()
        {
            await ReplyAsync(
             $"If you want to donate to PassiveModding and support this project here are his donation links:" +
             $"\n<https://www.paypal.me/PassiveModding/5usd>" +
             $"\n<https://goo.gl/vTtLg6>"
            );
        }

        [Command("dm"), Summary("dm 'hey babe'"), Remarks("Direct Messages the user")]
        public async Task Dmuser([Remainder] string message)
        {
            var dm = await Context.User.CreateDMChannelAsync();
            await dm.SendMessageAsync(message);
            await ReplyAsync("check your DMs babe :heart:");
        }

        [Command("revivechat"), Summary("revivechat"), Alias("revive", "wakeup"), Remarks("wakes everyone up (once every hour max)"), Ratelimit(1, 60, Measure.Minutes)]
        public async Task Revive()
            =>await ReplyAsync("pay attention my bitches, theres chatting to do @everyone");
    }
}