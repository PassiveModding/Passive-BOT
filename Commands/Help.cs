using Discord;
using Discord.Commands;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using PassiveBOT.Configuration;
using System.Collections.Generic;
using PassiveBOT.Services;

namespace PassiveBOT.Commands
{
    public class Help : ModuleBase
    {
        private CommandService _service;
        private PaginationService _paginator;

        public  Help(PaginationService paginator, CommandService service)
        {
            _paginator = paginator;
            _service = service;
        }

        [Command("command"), Summary("command 'meme'"), Remarks("all help commands")]
        public async Task HelpAsync([Remainder, Optional]string command)
        {
                var result = _service.Search(Context, command);

                if (!result.IsSuccess)
                    await ReplyAsync($"**Command Name:** {command}\n**Error:** Not Found!\n**Reason:** Wubbalubbadubdub!");
                var builder = new EmbedBuilder()
                {
                    Color = new Discord.Color(179, 56, 216)
                };

                foreach (var match in result.Commands)
                {
                    var cmd = match.Command;
                    builder.Title = cmd.Name.ToUpper();
                    builder.Description = $"**Aliases:** {string.Join(", ", cmd.Aliases)}\n**Parameters:** {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                        $"**Remarks:** {cmd.Remarks}\n**Summary:** `{Config.Load().Prefix}{cmd.Summary}`";
                }
                await ReplyAsync("", false, builder.Build());
        }

        [Command("help"), Summary("help 'meme'"), Remarks("all help commands")]
        [Ratelimit(1, 15, Measure.Seconds)]
        public async Task Help2Async()
        {
            var pages = new List<string>{};

            foreach (var module in _service.Modules)
            {
                string description = null;
                foreach (var cmd in module.Commands)
                {
                    var result = module.Name;

                    if (result == "Owner") { }
                    else
                        description += $"{Config.Load().Prefix}{cmd.Aliases.First()} - {cmd.Remarks}\n";
                }

                if (!string.IsNullOrWhiteSpace(description))
                    pages.Add($"**{module.Name}**\n{description}");
            }
            var message = new PaginatedMessage(pages, "Help Commands", new Color(0xb100c1), Context.User);

            await _paginator.SendPaginatedMessageAsync(Context.Channel, message);
        }

        [Command("faq"), Alias("helpme"), Summary("helpme"), Remarks("Used for the FAQ link for PassiveBOT")]
        public async Task Helpme()
        => await ReplyAsync($"For the FAQ section, please check out the FAQ page on our site: {Load.faq} \nalso please feel free to ask additional questions here");


        [Command("donate"), Summary("donate"), Alias("support"), Remarks("Donation Links for PassiveModding")]
        public async Task Donate()
        {
            await ReplyAsync(
             $"If you want to donate to PassiveModding and support this project here are his donation links:" +
             $"\n<https://www.paypal.me/PassiveModding/5usd>" +
             $"\n<https://goo.gl/vTtLg6>"
            );
        }

        /*
        [Command("nottest")]
        public async Task NotTest()
        {
            await ReplyAsync(".test");
        }

        [Command("test")]
        public async Task Test()
        {
            await ReplyAsync($"Author: {Context.Message.Author.Id} Me: {Context.Client.CurrentUser.Id}");
            if (Context.Message.Author.Id == Context.Client.CurrentUser.Id)
            {
                await ReplyAsync("this was sent by me");
            }
            else
                await ReplyAsync("this was not sent by me");
        }*/

    }
}