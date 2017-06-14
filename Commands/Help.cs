using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;
using PassiveBOT.preconditions;
using PassiveBOT.Services;

namespace PassiveBOT.Commands
{
    public class Help : ModuleBase
    {
        private readonly PaginationService _paginator;
        private readonly CommandService _service;

        public Help(PaginationService paginator, CommandService service)
        {
            _paginator = paginator;
            _service = service;
        }

        [Command("command")]
        [Summary("command 'meme'")]
        [Remarks("all help commands")]
        public async Task HelpAsync([Remainder] string command = null)
        {
            var result = _service.Search(Context, command);

            if (command == null)
            {
                await ReplyAsync($"Please specify a command, ie '{Load.Pre}command kick'");
                return;
            }

            if (!result.IsSuccess)
                await ReplyAsync($"**Command Name:** {command}\n**Error:** Not Found!\n**Reason:** Wubbalubbadubdub!");
            var builder = new EmbedBuilder
            {
                Color = new Color(179, 56, 216)
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;
                builder.Title = cmd.Name.ToUpper();
                builder.Description =
                    $"**Aliases:** {string.Join(", ", cmd.Aliases)}\n**Parameters:** {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                    $"**Remarks:** {cmd.Remarks}\n**Summary:** `{Load.Pre}{cmd.Summary}`";
            }
            await ReplyAsync("", false, builder.Build());
        }

        [Command("help", RunMode = RunMode.Async)]
        [Summary("help 'meme'")]
        [Remarks("all help commands")]
        [Ratelimit(1, 15, Measure.Seconds)]
        public async Task Help2Async()
        {
            if (Context.Channel is IDMChannel)
            {
                var builder = new EmbedBuilder
                {
                    Color = new Color(114, 137, 218),
                    Title = $"I am PassiveBOT Created By {Load.Owner} <{Load.Siteurl}>, Here are my commands:"
                };

                foreach (var module in _service.Modules)
                {
                    var description =
                        (from cmd in module.Commands let result = module.Name where result != "Owner" select cmd)
                        .Aggregate<CommandInfo, string>(null,
                            (current, cmd) => current + $"{Load.Pre}{cmd.Aliases.First()} - {cmd.Remarks}\n");

                    if (!string.IsNullOrWhiteSpace(description))
                        builder.AddField(x =>
                        {
                            x.Name = module.Name;
                            x.Value = description;
                        });
                }
                await ReplyAsync("", false, builder.Build());
            }
            else
            {
                var pages = new List<string>();

                foreach (var module in _service.Modules)
                {
                    string description = null;
                    foreach (var cmd in module.Commands)
                    {
                        var result = module.Name;

                        if (result != "Owner")
                            description += $"{Load.Pre}{cmd.Aliases.First()} - {cmd.Remarks}\n";
                    }

                    if (!string.IsNullOrWhiteSpace(description))
                        pages.Add($"**{module.Name}**\n{description}");
                }
                var message = new PaginatedMessage(pages, "Help Commands", new Color(0xb100c1), Context.User);

                await _paginator.SendPaginatedMessageAsync(Context.Channel, message);
            }
        }


        [Command("faq")]
        [Alias("helpme")]
        [Summary("helpme")]
        [Remarks("Used for the FAQ link for PassiveBOT")]
        public async Task Helpme()
        {
            await ReplyAsync(
                $"For the FAQ section, please check out the FAQ page on our site: {Load.Faq} \nalso please feel free to ask additional questions here");
        }


        [Command("donate")]
        [Summary("donate")]
        [Alias("support")]
        [Remarks("Donation Links for PassiveModding")]
        public async Task Donate()
        {
            await ReplyAsync(
                "If you want to donate to PassiveModding and support this project here are his donation links:" +
                "\n<https://www.paypal.me/PassiveModding/5usd>" +
                "\n<https://goo.gl/vTtLg6>"
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