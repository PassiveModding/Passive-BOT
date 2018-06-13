namespace PassiveBOT.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.Addons.Interactive;
    using global::Discord.Commands;

    using PassiveBOT.Discord.Context;
    using PassiveBOT.Discord.Extensions;

    /// <summary>
    /// The help module
    /// </summary>
    public class Help : Base
    {
        /// <summary>
        /// The command service
        /// </summary>
        private readonly CommandService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="Help"/> class.
        /// </summary>
        /// <param name="commandservice">
        /// The commandservice.
        /// </param>
        private Help(CommandService commandservice)
        {
            service = commandservice;
        }

        /// <summary>
        /// Gets or sets the current command being executed
        /// </summary>
        private CommandInfo Command { get; set; }

        /// <summary>
        /// The help command.
        /// </summary>
        /// <param name="checkForMatch">
        /// The checkForMatch.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("Help")]
        [Summary("Lists all accessible commands")]
        [Remarks("Use FullHelp for all commands")]
        public async Task HelpCommand([Remainder] string checkForMatch = null)
        {
            await GenerateHelp(checkForMatch);
        }

        /// <summary>
        /// The full help.
        /// </summary>
        /// <param name="checkForMatch">
        /// The check for match.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("FullHelp")]
        public async Task FullHelp([Remainder] string checkForMatch = null)
        {
            await GenerateHelp(checkForMatch, false);
        }

        /// <summary>
        /// Generates a help message
        /// </summary>
        /// <param name="checkForMatch">Matching module name or command name</param>
        /// <param name="checkPreconditions">Whether or not to display commands the user does not have access to</param>
        /// <returns>Task Finished</returns>
        /// <exception cref="Exception">
        /// Throws if command specified and no match is found
        /// </exception>
        public async Task GenerateHelp(string checkForMatch = null, bool checkPreconditions = true)
        {
            if (checkForMatch == null)
            {
                var pages = new List<PaginatedMessage.Page>();
                var i = 1;
                var fields = new List<EmbedFieldBuilder>
                                     {
                                         new EmbedFieldBuilder
                                             {
                                                 Name = $"[{i}] Commands Summary",
                                                 Value = "Go to the respective page number of each module to view the commands in more detail. " +
                                                         "You can react with the :1234: emote and type a page number to go directly to that page too," +
                                                         "otherwise react with the arrows to change pages."
                                             }
                                     };
                var modules = service.Modules.OrderBy(x => x.Name).ToList();

                var pageContents = new Dictionary<string, List<string>>();

                foreach (var module in modules)
                {
                    var passingCommands = checkPreconditions ? module.Commands.Where(x => x.CheckPreconditionsAsync(Context, Context.Provider).Result.IsSuccess).ToList() : module.Commands;

                    if (!passingCommands.Any())
                    {
                        continue;
                    }

                    fields.Add(new EmbedFieldBuilder
                    {
                        Name = module.Name,
                        Value = string.Join(", ", passingCommands.Select(x => x.Aliases.FirstOrDefault()).Where(x => x != null).ToList())
                    });

                    pageContents.Add(module.Name, passingCommands.Select(x => $"{Context.Prefix}{x.Aliases.FirstOrDefault()} {string.Join(" ", x.Parameters.Select(CommandInformation.ParameterInformation))}").ToList());
                    i++;
                }

                pages.Add(new PaginatedMessage.Page
                {
                    Fields = fields,
                    Title = $"{Context.Client.CurrentUser.Username} Commands"
                });

                foreach (var contents in pageContents)
                {
                    var splitFields = TextManagement.SplitList(contents.Value, 10)
                        .Select(x => new EmbedFieldBuilder
                        {
                            Name = contents.Key,
                            Value = string.Join("\n", x)
                        }).ToList();
                    pages.Add(new PaginatedMessage.Page
                    {
                        Fields = splitFields
                    });
                }

                await PagedReplyAsync(new PaginatedMessage { Pages = pages, Title = $"{Context.Client.CurrentUser.Username} Help || Prefix: {Context.Prefix}", Color = Color.DarkRed }, new ReactionList { Backward = true, Forward = true, Jump = true, Trash = true });
            }
            else
            {
                var module = service.Modules.FirstOrDefault(x => string.Equals(x.Name, checkForMatch, StringComparison.CurrentCultureIgnoreCase));
                var fields = new List<EmbedFieldBuilder>();
                if (module != null)
                {
                    var passingCommands = checkPreconditions ? module.Commands.Where(x => x.CheckPreconditionsAsync(Context, Context.Provider).Result.IsSuccess).ToList() : module.Commands;
                    if (!passingCommands.Any())
                    {
                        throw new Exception("No Commands available with your current permission level.");
                    }

                    var info = passingCommands.Select(x => $"{Context.Prefix}{x.Aliases.FirstOrDefault()} {string.Join(" ", x.Parameters.Select(CommandInformation.ParameterInformation))}").ToList();
                    var splitFields = TextManagement.SplitList(info, 10)
                        .Select(x => new EmbedFieldBuilder
                        {
                            Name = $"Module: {module.Name}",
                            Value = string.Join("\n", x)
                        }).ToList();
                    fields.AddRange(splitFields);
                }

                var command = service.Search(Context, Context.Message.Content.Substring(Command.Aliases.First().Length + Context.Prefix.Length + 1)).Commands?.FirstOrDefault().Command;
                if (command != null)
                {
                    if (command.CheckPreconditionsAsync(Context, Context.Provider).Result.IsSuccess)
                    {
                        fields.Add(new EmbedFieldBuilder
                        {
                            Name = $"Command: {command.Name}",
                            Value = "**Usage:**\n" +
                                                   $"{Context.Prefix}{command.Aliases.FirstOrDefault()} {string.Join(" ", command.Parameters.Select(CommandInformation.ParameterInformation))}\n" +
                                                   "**Aliases:**\n" +
                                                   $"{string.Join("\n", command.Aliases)}\n" +
                                                   "**Module:**\n" +
                                                   $"{command.Module.Name}\n" +
                                                   "**Summary:**\n" +
                                                   $"{command.Summary ?? "N/A"}\n" +
                                                   "**Remarks:**\n" +
                                                   $"{command.Remarks ?? "N/A"}"
                        });
                    }
                }

                if (!fields.Any())
                {
                    throw new Exception("There are no matches for this input.");
                }

                await InlineReactionReplyAsync(new ReactionCallbackData(string.Empty, new EmbedBuilder
                {
                    Fields = fields,
                    Color = Color.DarkRed
                }.Build(), timeout: TimeSpan.FromMinutes(5))
                .WithCallback(new Emoji("❌"),
                        async (c, r) =>
                            {
                                await r.Message.Value.DeleteAsync();
                                await c.Message.DeleteAsync();
                            }));
            }
        }
        
        /// <summary>
        /// Runs before executing the command and sets the 'Command'
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        protected override void BeforeExecute(CommandInfo command)
        {
            Command = command;
            base.BeforeExecute(command);
        }
    }
}
