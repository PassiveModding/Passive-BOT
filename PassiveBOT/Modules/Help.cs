namespace PassiveBOT.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.Addons.Interactive;
    using global::Discord.Commands;

    using PassiveBOT.Discord.Context;
    using PassiveBOT.Discord.Extensions;

    using Sparrow.Platform.Posix;

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
        /// The help command.
        /// </summary>
        /// <param name="checkForMatch">
        /// The checkForMatch.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("help")]
        public async Task HelpCommand([Remainder] string checkForMatch = null)
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
                    var passingCommands = module.Commands.Where(x => x.CheckPreconditionsAsync(Context, Context.Provider).Result.IsSuccess).ToList();

                    if (!passingCommands.Any())
                    {
                        continue;
                    }

                    fields.Add(new EmbedFieldBuilder
                    {
                        Name = module.Name,
                        Value = string.Join(", ", passingCommands.Select(x => x.Aliases.FirstOrDefault()).Where(x => x != null).ToList())
                    });

                    pageContents.Add(module.Name, passingCommands.Select(x => $"{Context.Prefix}{x.Aliases.FirstOrDefault()}{string.Join(" ", x.Parameters.Select(CommandInformation.ParameterInformation))}").ToList());
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

                await PagedReplyAsync(new PaginatedMessage { Pages = pages, Title = $"{Context.Client.CurrentUser.Username} Help || Prefix: {Context.Prefix}" }, new ReactionList { Backward = true, Forward = true, Jump = true, Trash = true });
            }
            else
            {
                var module = service.Modules.FirstOrDefault(x => string.Equals(x.Name, checkForMatch, StringComparison.CurrentCultureIgnoreCase));
                var fields = new List<EmbedFieldBuilder>();
                if (module != null)
                {
                    var passingCommands = module.Commands.Where(x => x.CheckPreconditionsAsync(Context, Context.Provider) == Task.FromResult(PreconditionResult.FromSuccess())).ToList();
                    if (!passingCommands.Any())
                    {
                        throw new Exception("No Commands available with your current permission level.");
                    }

                    var info = passingCommands.Select(x => $"{Context.Prefix}{x.Aliases.FirstOrDefault()}{string.Join(" ", x.Parameters.Select(CommandInformation.ParameterInformation))}").ToList();
                    var splitFields = TextManagement.SplitList(info, 10)
                        .Select(x => new EmbedFieldBuilder
                                         {
                                             Name = module.Name,
                                             Value = string.Join("\n", x)
                                         }).ToList();
                    fields.AddRange(splitFields);
                }

                var command = service.Commands.FirstOrDefault(x => string.Equals(x.Name, checkForMatch, StringComparison.CurrentCultureIgnoreCase));
                if (command != null)
                {
                    if (command.CheckPreconditionsAsync(Context, Context.Provider) == Task.FromResult(PreconditionResult.FromSuccess()))
                    {
                        fields.Add(new EmbedFieldBuilder
                                       {
                                           Name = command.Name,
                                           Value = $"**Usage:**\n" + 
                                                   $"{Context.Prefix}{command.Aliases.FirstOrDefault()}{string.Join(" ", command.Parameters.Select(CommandInformation.ParameterInformation))}\n" + 
                                                   $"**Aliases:**\n" + 
                                                   $"{string.Join("\n", command.Aliases)}\n" + 
                                                   $"**Module:**\n" + 
                                                   $"{command.Module}\n" + 
                                                   $"**Summary:**" + 
                                                   $"{command.Summary ?? "N/A"}\n" + 
                                                   $"**Remarks:**" + 
                                                   $"{command.Remarks ?? "N/A"}"
                        });
                    }
                }

                await InlineReactionReplyAsync(new ReactionCallbackData(null, new EmbedBuilder { Fields = fields }.Build(), true).WithCallback(new Emoji("❌"), (c, r) => c.Message.DeleteAsync()));
            }
        }
    }
}
