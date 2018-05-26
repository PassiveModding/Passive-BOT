using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PassiveBOT.Discord.Context;
using PassiveBOT.Discord.Context.Interactive.Paginator;
using PassiveBOT.Discord.Extensions;
using PassiveBOT.Handlers;
using PassiveBOT.Models;

namespace PassiveBOT.Modules.Info
{
    public class Help : Base
    {
        private readonly CommandService _service;

        private Help(CommandService service)
        {
            _service = service;
        }


        [Command("Command")]
        [Summary("Command")]
        [Remarks("Get specific info about a command")]
        public async Task Command([Remainder]string name)
        {
            var result = _service.Search(Context, name);

            string desc;
            if (result.Error == CommandError.UnknownCommand)
            {
                desc = "**Command:** N/A";
            }
            else
            {
                var cmd = result.Commands.FirstOrDefault();

                desc = $"**Command Name:** `{cmd.Command.Name}`\n" +
                        $"**Summary:** `{cmd.Command?.Summary ?? "N/A"}`\n" +
                        $"**Remarks:** `{cmd.Command?.Remarks ?? "N/A"}`\n" +
                        $"**Aliases:** {(cmd.Command.Aliases.Any() ? string.Join(" ", cmd.Command.Aliases.Select(x => $"`{x}`")) : "N/A")}\n" +
                        $"**Parameters:** {(cmd.Command.Parameters.Any() ? string.Join(" ", cmd.Command.Parameters.Select(x => x.IsOptional ? $" `<(Optional){x.Name}>` " : $" `<{x.Name}>` ")) : "N/A")}\n";
            }

            await SendEmbedAsync(new EmbedBuilder
            {
                Title = $"Command Lookup",
                Description = desc
            }.Build());

        }

        [Command("Help")]
        [Summary("Help")]
        [Remarks("Show a list of all commands with usage")]
        public async Task Helpc()
        {
            var p = Context.Channel is IDMChannel ? ConfigModel.Load().Prefix : (DatabaseHandler.GetGuild(Context.Guild.Id).Settings.Prefix.CustomPrefix ?? ConfigModel.Load().Prefix);
            var simplemodules = new List<modulesummary>();
            foreach (var module in _service.Modules)
            {
                if (module.Commands.Count <= 0) continue;
                if (module.Commands.All(x => !CheckPrecondition.preconditioncheck(Context, x, CommandHandler.Provider))) continue;

                var passingcommands = module.Commands.Where(x => CheckPrecondition.preconditioncheck(Context, x, CommandHandler.Provider)).ToList();
                var moduleprefix = !string.IsNullOrWhiteSpace(module.Aliases.FirstOrDefault()) ? $"{module.Aliases.FirstOrDefault()} " : null;
                simplemodules.Add(new modulesummary
                {
                    name = module.Name,
                    cmdnames = passingcommands.Select(x => x.Name).ToList(),
                    cmdinfo = passingcommands.Select(x => $"`{p}{moduleprefix}{x.Summary}` - {x.Remarks}").ToList()
                });
            }
            var pages = new List<PaginatedMessage.Page>();
            var i = 1;
            var initfields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder
                {
                    Name = $"[{i}] Commands Summary",
                    Value = "Go to the respective page number of each module to view the commands in more detail. " +
                            "You can react with the :1234: emote and type a page number to go directly to that page too," +
                            "otherwise react with the arrows to change pages."
                }
            };
            foreach (var module in simplemodules)
            {
                i++;
                initfields.Add(new EmbedFieldBuilder
                {
                    Name = $"[{i}] {module.name}",
                    Value = string.Join(", ", module.cmdnames)
                });
            }
            pages.Add(new PaginatedMessage.Page
            {
                Fields = initfields
            });
            foreach (var module in simplemodules)
            {
                pages.Add(new PaginatedMessage.Page
                {
                    dynamictitle = module.name,
                    description = string.Join("\n", module.cmdinfo)
                });
            }

            await PagedReplyAsync(new PaginatedMessage
            {
                Pages = pages
            }, new ReactionList
            {
                Backward = true,
                Forward = true,
                Jump = true,
                Trash = true
            });
        }

        public class modulesummary
        {
            public string name { get; set; }
            public List<string> cmdnames { get; set; } = new List<string>();
            public List<string> cmdinfo { get; set; }
        }

    }
}
