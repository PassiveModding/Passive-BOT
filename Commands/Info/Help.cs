using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;
using PassiveBOT.Discord.Addons.Interactive;
using PassiveBOT.Discord.Addons.Interactive.Paginator;

namespace PassiveBOT.Commands.Info
{
    public class Help : InteractiveBase
    {
        private readonly CommandService _service;

        public Help(CommandService service)
        {
            _service = service;
        }

        [Command("command")]
        [Summary("command <command name>")]
        [Remarks("get info about a specific command")]
        public async Task CommandAsync([Remainder] string command = null)
        {
            if (command == null)
            {
                await ReplyAsync($"Please specify a command, ie `{Load.Pre}command kick`");
                return;
            }

            try
            {
                var result = _service.Search(Context, command);
                var builder = new EmbedBuilder
                {
                    Color = new Color(179, 56, 216)
                };

                foreach (var match in result.Commands)
                {
                    var cmd = match.Command;
                    builder.Title = cmd.Name.ToUpper();
                    builder.Description +=
                        $"**Aliases:** {string.Join(", ", cmd.Aliases)}\n**Parameters:** {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                        $"**Remarks:** {cmd.Remarks}\n**Summary:** `{Load.Pre}{cmd.Summary}`\n";
                }

                await ReplyAsync("", false, builder.Build());
            }
            catch
            {
                await ReplyAsync($"**Command Name:** {command}\n**Error:** Not Found!");
            }
        }

        [Command("help")]
        [Summary("help")]
        [Remarks("all help commands")]
        public async Task HelpAsync([Remainder] string modulearg = null)
        {
            string isserver;
            if (Context.Channel is IPrivateChannel)
                isserver = Load.Pre;
            else
                try
                {
                    isserver = GuildConfig.GetServer(Context.Guild).Prefix;
                }
                catch
                {
                    isserver = Load.Pre;
                }

            var embed = new EmbedBuilder
            {
                Color = new Color(114, 137, 218),
                Title = $"PassiveBOT | Commands | Prefix: {isserver}"
            };
            if (modulearg == null) //ShortHelp
            {
                var moduleselect = new List<string>
                {
                    $"`1` - This Page",
                    $"`2` - List of all commands(1)",
                    $"`3` - List of all commands(2)"
                };
                var i = 2;
                foreach (var module in _service.Modules.Where(x => x.Commands.Count > 0))
                {
                    i++;
                    moduleselect.Add($"`{i}` - {module.Name}");
                }

                var pages = new List<PaginatedMessage.Page>
                {
                    new PaginatedMessage.Page
                    {
                        dynamictitle = $"PassiveBOT | Modules | Prefix: {isserver}",
                        description = $"Here is a list of all the PassiveBOT command modules\n" +
                                      $"Click the arrows to view each one!\n" +
                                      $"Or Click :1234: and reply with the page number you would like\n\n" +
                                      string.Join("\n", moduleselect)
                    },
                    new PaginatedMessage.Page
                    {
                        dynamictitle = $"PassiveBOT | All Commands | Prefix: {isserver}",
                        description = string.Join("\n", _service.Modules.Where(x => x.Commands.Count > 0).Take(8).Select(x => $"__**{x.Name}**__\n{string.Join(", ", x.Commands.Select(c => c.Name))}"))
                    },
                    new PaginatedMessage.Page
                    {
                        dynamictitle = $"PassiveBOT | All Commands | Prefix: {isserver}",
                        description = string.Join("\n", _service.Modules.Where(x => x.Commands.Count > 0).Skip(8).Select(x => $"__**{x.Name}**__\n{string.Join(", ", x.Commands.Select(c => c.Name))}"))
                    }
                };
                foreach (var module in _service.Modules.Where(x => x.Commands.Count > 0))
                {
                    var list = module.Commands.Select(command => $"`{isserver}{command.Summary}` - {command.Remarks}").ToList();
                    if (module.Commands.Count > 0)
                    {
                        pages.Add(new PaginatedMessage.Page
                        {
                            dynamictitle = module.Name,
                            description = string.Join("\n", list)
                        });
                    }
                }
                var msg = new PaginatedMessage
                {
                    Color = Color.Green,
                    Pages = pages
                };
                await PagedReplyAsync(msg, showindex: true);
                return;
                //embed.AddField("\n\n**NOTE**",
                //    $"You can also see modules in more detail using `{isserver}help <modulename>`\n" +
                //    "Also Please consider supporting this project on patreon: <https://www.patreon.com/passivebot>");
            }

            var mod = _service.Modules.FirstOrDefault(x =>
                string.Equals(x.Name, modulearg, StringComparison.CurrentCultureIgnoreCase));

            if (mod == null)
            {
                var list = _service.Modules.Where(x => x.Commands.Count > 0).Select(x => x.Name);
                var response = string.Join("\n", list);
                embed.AddField("ERROR, Module not found", response);
                await ReplyAsync("", false, embed.Build());
                return;
            }

            var commands = mod.Commands.Select(x => $"`{isserver}{x.Summary}` - {x.Remarks}").ToList();
            embed.AddField($"{mod.Name} Commands", commands.Count == 0 ? "N/A" : string.Join("\n", commands));
            await ReplyAsync("", false, embed.Build());
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

        [Command("Suggest")]
        [Summary("suggest <suggestion>")]
        [Remarks("Suggest a feature or improvement etc.")]
        public async Task Suggest([Remainder] string suggestion = null)
        {
            if (suggestion == null)
                await ReplyAsync("Please suggest something lol...");
            else
                try
                {
                    var s = Homeserver.Load().Suggestion;
                    var c = Context.Client.GetChannel(s);
                    var embed = new EmbedBuilder();
                    embed.AddField($"Suggestion from {Context.User.Username}", suggestion);
                    embed.WithFooter(x => { x.Text = $"{Context.Message.CreatedAt} || {Context.Guild.Name}"; });
                    embed.Color = Color.Blue;
                    await ((ITextChannel) c).SendMessageAsync("", false, embed.Build());
                    await ReplyAsync("Suggestion Sent!!");
                }
                catch
                {
                    await ReplyAsync("The bots owner has not yet configured the suggestion channel");
                }
        }

        [Command("Bug")]
        [Summary("Bug <bug>")]
        [Remarks("Report a bug.")]
        public async Task Bug([Remainder] string bug = null)
        {
            if (bug == null)
                await ReplyAsync("report a bug please.");
            else
                try
                {
                    var s = Homeserver.Load().Suggestion;
                    var c = Context.Client.GetChannel(s);
                    var embed = new EmbedBuilder();
                    embed.AddField($"BugReport from {Context.User.Username}", bug);
                    embed.WithFooter(x => { x.Text = $"{Context.Message.CreatedAt} || {Context.Guild.Name}"; });
                    embed.Color = Color.Red;
                    await ((ITextChannel) c).SendMessageAsync("", false, embed.Build());
                    await ReplyAsync("Bug Report Sent!!");
                }
                catch
                {
                    await ReplyAsync("The bots owner has not yet configured the Bug channel");
                }
        }
    }
}