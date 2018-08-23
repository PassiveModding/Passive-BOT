namespace PassiveBOT.Extensions.PassiveBOT
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Discord.Commands;
    using Discord.WebSocket;

    using global::PassiveBOT.Models;

    /// <summary>
    ///     Helps with logging of command and message statistics
    /// </summary>
    public class StatHelper
    {
        /*
        /// <summary>
        /// The message stats queue
        /// </summary>
        private static List<StatModel.MessageStat> messageStatsQueue = new List<StatModel.MessageStat>();
        */

        /// <summary>
        ///     Gets or sets the messages received since update.
        /// </summary>
        private static int messagesReceivedSinceUpdate;

        /// <summary>
        ///     Updates a command's uses in the stat-model
        /// </summary>
        /// <param name="command">
        ///     The command.
        /// </param>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="isError">
        ///     if the command used throws an error
        /// </param>
        public static void LogCommand(CommandInfo command, SocketUserMessage message, bool isError = false)
        {
            var model = StatModel.Load();
            var currentCommand = model.CommandStats.FirstOrDefault(x => x.CommandName == command.Aliases.FirstOrDefault());
            if (currentCommand == null)
            {
                // Ensure a guild is only logged IF the command is used in a guild
                var guilds = new List<StatModel.CommandStat.CommandGuild>();
                if (message.Author is SocketGuildUser gUser)
                {
                    guilds.Add(NewCommandGuild(gUser.Guild.Id));
                }

                // Initialize the command information
                model.CommandStats.Add(new StatModel.CommandStat { CommandName = command.Aliases.FirstOrDefault(), CommandUses = 1, CommandUsers = new List<StatModel.CommandStat.CommandUser> { NewCommandUser(message.Author.Id) }, CommandGuilds = guilds, ErrorCount = isError ? 1 : 0 });
            }
            else
            {
                currentCommand.CommandUses++;
                if (isError)
                {
                    currentCommand.ErrorCount++;
                }

                // Check to see if the user has used the command before
                var currentUser = currentCommand.CommandUsers.FirstOrDefault(x => x.UserId == message.Author.Id);
                if (currentUser == null)
                {
                    currentCommand.CommandUsers.Add(NewCommandUser(message.Author.Id));
                }
                else
                {
                    currentUser.Uses++;
                }

                if (message.Author is SocketGuildUser gUser)
                {
                    var commandGuild = currentCommand.CommandGuilds.FirstOrDefault(x => x.GuildId == gUser.Guild.Id);
                    if (commandGuild == null)
                    {
                        currentCommand.CommandGuilds.Add(NewCommandGuild(gUser.Guild.Id));
                    }
                    else
                    {
                        commandGuild.Uses++;
                    }
                }
            }

            model.Save();
        }

        /// <summary>
        /// Logs a message's stats to the stat file
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static Task LogMessageAsync(SocketUserMessage message)
        {
            messagesReceivedSinceUpdate++;
            
            if (messagesReceivedSinceUpdate > 500)
            {
                var model = StatModel.Load();
                model.MessageCount += 500;
                model.Save();
                messagesReceivedSinceUpdate = 0;
            }
            
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Creates a new command guild based on a guild Id
        /// </summary>
        /// <param name="guildId">
        ///     The guild id.
        /// </param>
        /// <returns>
        ///     The <see cref="StatModel.CommandStat.CommandGuild" />.
        /// </returns>
        public static StatModel.CommandStat.CommandGuild NewCommandGuild(ulong guildId)
        {
            return new StatModel.CommandStat.CommandGuild { GuildId = guildId, Uses = 1 };
        }

        /// <summary>
        ///     Creates a new command user
        /// </summary>
        /// <param name="userId">
        ///     The user id.
        /// </param>
        /// <returns>
        ///     The <see cref="StatModel.CommandStat.CommandUser" />.
        /// </returns>
        public static StatModel.CommandStat.CommandUser NewCommandUser(ulong userId)
        {
            return new StatModel.CommandStat.CommandUser { UserId = userId, Uses = 1 };
        }
    }
}