using Discord;
using Discord.Commands;
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