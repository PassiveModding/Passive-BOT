using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.preconditions;

namespace PassiveBOT.Commands
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

            var user = msg.Author;
            var time = msg.Timestamp;

            var embed = new EmbedBuilder
            {
                Title = user.Username,
                Description = msg.ToString(),
                Timestamp = time
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

        [Command("hug")]
        [Summary("hug <@user>")]
        [Remarks("Gives a big sloppy hug")]
        public async Task Huguser(IUser user)
        {
            await ReplyAsync($"Hey {user.Mention}, {Context.User.Mention} sent you a big warm hug :heart:");
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