using System;
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
        private readonly CommandService _service;

        public Help(CommandService service)
        {
            _service = service;
        }

        [Command("command")]
        [Summary("command <command name>")]
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

        [Command("help")]
        [Summary("help")]
        [Remarks("all help commands")]
        public async Task Help2Async(string modulename = null)
        {
            if (modulename == null)
            {
                var builder = new EmbedBuilder
                {
                    Color = new Color(114, 137, 218),
                    Title = $"I am PassiveBOT Created By {Load.Owner}"
                };
                var list = new List<string>();
                foreach (var module in _service.Modules)
                {
                    list.Add(module.Name);
                }
                builder.AddField("Modules", string.Join("\n", list.ToArray()));
                builder.AddField("Commands",
                    $"Type `{Load.Pre}help <modulename>` to see a list of commands in each module\n" +
                    $"or type `{Load.Pre}help all` in a direct message for all commands");
                await ReplyAsync("", false, builder.Build());
            }
            else if (modulename.ToLower() == "all")
            {

                var builder = new EmbedBuilder
                {
                    Color = new Color(114, 137, 218),
                    Title = $"I am PassiveBOT Created By {Load.Owner} <{Load.Siteurl}>, Here are my commands:"
                };
                foreach (var module in _service.Modules)
                {
                    string description = null;
                    foreach (var cmd in module.Commands)
                    {
                        var result = module.Name;
                        if (result != "Owner") description = description + $"{Load.Pre}{cmd.Aliases.First()} - {cmd.Remarks}\n";
                    }

                    if (!string.IsNullOrWhiteSpace(description))
                        builder.AddField(x =>
                        {
                            x.Name = module.Name;
                            x.Value = description;
                        });


                }
                if (!(Context.Channel is IDMChannel))
                {
                    await ReplyAsync(
                        $"Hey {Context.User.Mention}, I have sent my full command list to you in a direct message! <3");
                }
                    await Context.User.SendMessageAsync("", false, builder.Build());
            }
            else
            {
                var builder = new EmbedBuilder
                {
                    Color = new Color(114, 137, 218),
                    Title = $"I am PassiveBOT Created By {Load.Owner}\nHere are my commands:"
                };
                var list = new List<string>();

                foreach (var module in _service.Modules)
                {
                    if (string.Equals(module.Name, modulename, StringComparison.OrdinalIgnoreCase))
                    {
                        var description = module.Commands.Select(cmd => $"{Load.Pre}{cmd.Summary} - {cmd.Remarks}").ToList();
                        if (description.ToString() != "")
                        {
                            builder.AddField(x =>
                            {
                                x.Name = module.Name;
                                x.Value = string.Join("\n", description.ToArray());
                            });
                        }


                    }
                    list.Add(module.Name);
                }
                if (builder.Fields.Count == 0)
                {
                    var embed = new EmbedBuilder
                    {
                        Color = new Color(114, 137, 218),
                        Title = $"I am PassiveBOT Created By {Load.Owner}"
                    };
                    embed.AddField("Error", $"The Module **{modulename}** does not exist\n" +
                                            $"Type `{Load.Pre}help <modulename>` using one of the modules below for a list of those commands\n" +
                                            $"or type `{Load.Pre}help all` for a list of all commands to be sent to your DMs");
                    embed.AddField("Modules", string.Join("\n", list.ToArray()));
                    await ReplyAsync("", false, embed.Build());
                    return;
                }
                builder.AddField("Other Modules", string.Join("\n", list.ToArray()));
                await ReplyAsync("", false, builder.Build());


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