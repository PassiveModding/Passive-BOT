using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.strings;

namespace PassiveBOT.Commands
{
    public class RandomStuff : ModuleBase
    {
        [Command("rip")]
        [Summary("rip")]
        [Remarks("rip in pepperoni")]
        public async Task Rip(IUser user = null)
        {
            await ReplyAsync(
                $"rip in pepperoni {(user == null ? "little boi" : user.Mention)}, you will forever be remembered in the hall of lame");
        }

        [Command("rekt")]
        [Summary("rekt")]
        [Remarks("rekt harder than your mom")]
        public async Task Rekt()
        {
            await ReplyAsync("Boi you just got destroyed harder than your mom last night xD");
        }

        [Command("idk")]
        [Summary("idk")]
        [Alias("shrug")]
        [Remarks("I don't Know")]
        public async Task Idk()
        {
            await ReplyAsync(@"¯\_(ツ)_/¯");
        }

        [Command("gg")]
        [Summary("gg")]
        [Remarks("good game")]
        public async Task Gg()
        {
            await ReplyAsync("good game young chap");
        }

        [Command("kat")]
        [Summary("kat")]
        [Remarks("kitty kat")]
        public async Task Kat()
        {
            await ReplyAsync("ʢ◉ᴥ◉ʡ");
        }

        [Command("fight")]
        [Summary("fight")]
        [Remarks("fight me")]
        public async Task Fight()
        {
            await ReplyAsync("(ง ◔ _ ◔ )ง");
        }

        [Command("wtf")]
        [Summary("wtf")]
        [Remarks("wtf")]
        public async Task Wtf()
        {
            await ReplyAsync("ಠ_ಠ");
        }

        [Command("happy")]
        [Summary("happy")]
        [Remarks("happy face")]
        public async Task Happy()
        {
            await ReplyAsync("^‿^");
        }

        [Command("lenny")]
        [Summary("lenny")]
        [Remarks("lenny")]
        public async Task Lenny()
        {
            await ReplyAsync("( ͡° ͜ʖ ͡°)");
        }

        [Command("spam")]
        [Summary("spam")]
        [Remarks("just some spam")]
        public async Task Spam()
        {
            await ReplyAsync("https://cdn.discordapp.com/attachments/303375479317069824/306355062395895808/spam.png");
        }

        [Command("roast")]
        [Summary("roast")]
        [Alias("insult")]
        [Remarks("who doesnt like to be insulted")]
        public async Task Insult()
        {
            var rnd = new Random();
            await ReplyAsync(FunStr.Insult[rnd.Next(0, FunStr.Insult.Length)]);
        }

        [Command("cringe")]
        [Summary("cringe")]
        [Alias("suicide", "kms", "kys")]
        [Remarks("Sadness")]
        public async Task Cringe()
        {
            await ReplyAsync(
                "```" +
                "     _______      \n" +
                "    |/      |     \n" +
                "    |   ( ͡° ͜ʖ ͡°)  \n" +
                "    |     _-|-_   \n" +
                "    |       |     \n" +
                "    |      / L    \n" +
                "    |             \n" +
                "   _|___          \n" +
                "```"
            );
        }

        [Command("dick")]
        [Summary("dick")]
        [Alias("size", "penis")]
        [Remarks("Compare Sizes")]
        public async Task Dick()
        {
            var rnd = new Random();
            await ReplyAsync(FunStr.Dick[rnd.Next(0, FunStr.Dick.Length)]);
        }
    }
}