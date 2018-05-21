using System.Threading.Tasks;
using Discord.Commands;
using PassiveBOT.Discord.Context;

namespace PassiveBOT.Modules.Text
{
    public class Chat : Base
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
    }
}