using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Configuration;

namespace PassiveBOT.Commands
{
    public class Help : ModuleBase
    {
        private readonly CommandService _service;

        public Help(CommandService service)
        {
            _service = service;
        }

        [Command("command")]
        [Summary("command <command name>")]
        [Remarks("all help commands")]
        public async Task CommandAsync([Remainder] string command = null)
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

        [Command("help")]
        [Summary("help")]
        [Remarks("all help commands")]
        public async Task HelpAsync([Remainder] string modulearg = null)
        {
            var embed = new EmbedBuilder
                {
                    Color = new Color(114, 137, 218),
                    Title = "PassiveBOT | Commands"
                };
            string isserver;
            if (Context.Channel is IPrivateChannel)
            {
                isserver = Load.Pre;
            }
            else
            {
                isserver = GuildConfig.Load(Context.Guild.Id).Prefix;
            }
            if (modulearg == null) //ShortHelp
            {

                foreach (var module in _service.Modules)
                {
                    var list = module.Commands.Select(command => command.Name).ToList();
                    if (module.Commands.Count > 0)
                            embed.AddField(x =>
                            {
                                x.Name = module.Name;
                                x.Value = string.Join(", ", list);
                            });
                }
                embed.AddField("\n\n**NOTE**", $"You can also see modules in more detail using `{isserver}help <modulename>`\n" +
                                           $"Also Please consider supporting this project on patreon: <https://www.patreon.com/passivebot>");
            }
            else
            {
                foreach (var module in _service.Modules)
                {
                    if (module.Name.ToLower() == modulearg.ToLower())
                    {
                        var list = new List<string>();
                        foreach (var command in module.Commands)
                        {
                            list.Add(
                                $"{isserver}{command.Summary} - {command.Remarks}");
                        }
                        embed.AddField(module.Name, string.Join("\n", list));
                    }
                }
                if (embed.Fields.Count == 0)
                {
                    embed.AddField("Error", $"{modulearg} is not a module");
                    var list = _service.Modules.Select(module => module.Name).ToList();
                    embed.AddField("Modules", string.Join("\n", list));
                }

                
            }
            await ReplyAsync("", false, embed.Build());
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

        [Command("Suggest")]
        [Summary("suggest <suggestion>")]
        [Remarks("Suggest a feature or improvement etc.")]
        public async Task Suggest([Remainder] string suggestion = null)
        {
            if (suggestion == null)
            {
                await ReplyAsync("Please suggest something lol...");
            }
            else
            {
                try
                {
                    var s = Homeserver.Load().Suggestion;
                    var c = await Context.Client.GetChannelAsync(s);
                    var embed = new EmbedBuilder();
                    embed.AddField($"Suggestion from {Context.User.Username}", suggestion);
                    embed.WithFooter(x =>
                    {
                        x.Text = $"{Context.Message.CreatedAt} || {Context.Guild.Name}";
                    });
                    embed.Color = Color.Blue;
                    await (c as ITextChannel).SendMessageAsync("", false, embed.Build());
                    await ReplyAsync("Suggestion Sent!!");
                }
                catch
                {
                    await ReplyAsync("The bots owner has not yet configured the suggestion channel");
                }

            }
        }

        [Command("Bug")]
        [Summary("Bug <bug>")]
        [Remarks("Report a bug.")]
        public async Task Bug([Remainder] string bug = null)
        {
            if (bug == null)
            {
                await ReplyAsync("report a bug please.");
            }
            else
            {
                try
                {
                    var s = Homeserver.Load().Suggestion;
                    var c = await Context.Client.GetChannelAsync(s);
                    var embed = new EmbedBuilder();
                    embed.AddField($"BugReport from {Context.User.Username}", bug);
                    embed.WithFooter(x =>
                    {
                        x.Text = $"{Context.Message.CreatedAt} || {Context.Guild.Name}";
                    });
                    embed.Color = Color.Red;
                    await (c as ITextChannel).SendMessageAsync("", false, embed.Build());
                    await ReplyAsync("Bug Report Sent!!");
                }
                catch
                {
                    await ReplyAsync("The bots owner has not yet configured the Bug channel");
                }

            }
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