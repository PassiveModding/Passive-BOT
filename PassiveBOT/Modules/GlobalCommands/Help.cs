namespace PassiveBOT.Modules.GlobalCommands
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
    using PassiveBOT.Handlers;

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
        public Task HelpCommandAsync([Remainder] string checkForMatch = null)
        {
            return GenerateHelpAsync(checkForMatch);
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
        [Summary("Lists all commands")]
        public Task FullHelpAsync([Remainder] string checkForMatch = null)
        {
            return GenerateHelpAsync(checkForMatch, false);
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
        public async Task GenerateHelpAsync(string checkForMatch = null, bool checkPreconditions = true)
        {
            try
            {
                if (checkForMatch == null)
                {
                    await PagedHelpAsync(checkPreconditions);
                }
                else
                {
                    await ModuleCommandHelpAsync(checkPreconditions, checkForMatch);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// The module/command help.
        /// </summary>
        /// <param name="checkPreconditions">
        /// The check preconditions.
        /// </param>
        /// <param name="checkForMatch">
        /// The check for match.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// throws if none found
        /// </exception>
        public Task ModuleCommandHelpAsync(bool checkPreconditions, string checkForMatch)
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

            return InlineReactionReplyAsync(new ReactionCallbackData(string.Empty, new EmbedBuilder
                                                                                       {
                                                                                           Fields = fields,
                                                                                           Color = Color.DarkRed
                                                                                       }.Build(), timeout: TimeSpan.FromMinutes(5))
                .WithCallback(new Emoji("❌"),
                    async (c, r) =>
                        {
                            await r.Message.Value?.DeleteAsync();
                            await c.Message.DeleteAsync();
                        }));
        }

        /// <summary>
        /// The paged help.
        /// </summary>
        /// <param name="checkPreconditions">
        /// The check preconditions.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task PagedHelpAsync(bool checkPreconditions)
        {
            var pages = new List<PaginatedMessage.Page>();
            var moduleIndex = 1;

            // This ensures that we filter out all modules where the user cannot access ANY commands
            var modules = checkPreconditions ? 
                              service.Modules.OrderBy(x => x.Name).Where(x => x.Commands.Any(c => c.CheckPreconditionsAsync(Context, Context.Provider).Result.IsSuccess)).ToList() : 
                              service.Modules.OrderBy(x => x.Name).ToList();
            
            // Split the modules into groups of 5 to ensure the message doesn't get too long
            var moduleSets = TextManagement.SplitList(modules, 5);
            moduleIndex += moduleSets.Count - 1;
            var fields = new List<EmbedFieldBuilder>
                                     {
                                         new EmbedFieldBuilder
                                             {
                                                 // This gives a brief overview of how to use the paginated message and help commands.
                                                 Name = $"[1-{moduleIndex}] Commands Summary",
                                                 Value = "Go to the respective page number of each module to view the commands in more detail. " +
                                                         "You can react with the :1234: emote and type a page number to go directly to that page too,\n" +
                                                         "otherwise react with the arrows (◀ ▶) to change pages.\n" +
                                                         "For more info on modules or commands,\n" +
                                                         $"type `{Context.Prefix}help <ModuleName>` or `{Context.Prefix}help <CommandName>`"
                                             }
                                     };

            var pageContents = new Dictionary<string, List<string>>();
            var setIndex = 1;

            foreach (var moduleSet in moduleSets)
            {
                // Go through each module (in the sets of 5)
                foreach (var module in moduleSet)
                {
                    // Ensure that the user only gets to see commands they have access to
                    var passingCommands = checkPreconditions ? module.Commands.Where(x => x.CheckPreconditionsAsync(Context, Context.Provider).Result.IsSuccess).ToList() : module.Commands;

                    // If there are no commands that passed the check, continue to the next module.
                    if (!passingCommands.Any())
                    {
                        continue;
                    }

                    moduleIndex++;

                    // Add a new embed field with the info about our module and a list of all the command names
                    fields.Add(new EmbedFieldBuilder { Name = $"[{moduleIndex}] {module.Name}", Value = string.Join(", ", passingCommands.Select(x => x.Aliases.FirstOrDefault()).Where(x => x != null).ToList()) });

                    try
                    {
                        // This gives us the prefix, command name and all parameters to the command.
                        var summary = passingCommands.Select(x => $"{Context.Prefix}{x.Aliases.FirstOrDefault()} {string.Join(" ", x.Parameters.Select(CommandInformation.ParameterInformation))}").ToList();

                        if (!string.IsNullOrEmpty(module.Summary))
                        {
                            summary.Add($"**Summary**\n{module.Summary}");
                        }

                        // Add a full page summary to our 'PageContents' list for later use
                        pageContents.Add(module.Name, summary);
                    }
                    catch (Exception e)
                    {
                        // Note this should only throw IF there are two modules with the same name in the bot.
                        LogHandler.LogMessage(e.ToString(), LogSeverity.Error);
                    }
                }
                
                // Add the page for each Module Set to our pages list.
                pages.Add(new PaginatedMessage.Page
                {
                    Fields = fields,
                    Title = $"{Context.Client.CurrentUser.Username} Commands {setIndex}"
                });

                // Reset the fields list for the next module set
                fields = new List<EmbedFieldBuilder>();
                setIndex++;
            }

            // Now add each page with the full info with parameters 
            foreach (var contents in pageContents)
            {
                // Split these into groups of 10 to ensure there is no embed field character limit being hit. (1024 characters bet field description)
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

            return PagedReplyAsync(new PaginatedMessage { Pages = pages, Title = $"{Context.Client.CurrentUser.Username} Help || Prefix: {Context.Prefix}", Color = Color.DarkRed }, new ReactionList { Backward = true, Forward = true, Jump = true, Trash = true });
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
