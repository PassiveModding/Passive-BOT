namespace PassiveBOT.Discord.Extensions.PassiveBOT
{
    using System.Collections.Generic;
    using System.Linq;

    using global::Discord.Commands;
    using global::Discord.WebSocket;

    using global::PassiveBOT.Models;

    /// <summary>
    /// Helps with logging of command and message statistics
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
        /// Gets or sets the messages received since update.
        /// </summary>
        private static int messagesReceivedSinceUpdate;

        /// <summary>
        /// Updates a command's uses in the stat-model
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="isError">
        /// if the command used throws an error
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
                model.CommandStats.Add(new StatModel.CommandStat
                {
                    CommandName = command.Aliases.FirstOrDefault(),
                    CommandUses = 1,
                    CommandUsers = new List<StatModel.CommandStat.CommandUser>
                                                                  {
                                                                      NewCommandUser(message.Author.Id)
                                                                  },
                    CommandGuilds = guilds,
                    ErrorCount = isError ? 1 : 0
                });
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
        public static void LogMessage(SocketUserMessage message)
        {
            messagesReceivedSinceUpdate++;

            if (messagesReceivedSinceUpdate > 500)
            {
                var model = StatModel.Load();
                model.MessageCount += messagesReceivedSinceUpdate;
                model.Save();
                messagesReceivedSinceUpdate = 0;
            }

            /*
            // Queue the message in the message stats queue
            messageStatsQueue.Add(new StatModel.MessageStat
                                      {
                                          MessageLength = message.Content.Length,
                                          MessageGuild = message.Author is SocketGuildUser gUser ? gUser.Guild.Id : 0,
                                          MessageOwner = message.Author.Id
                                      });

            // Ensure that the file is only updates periodically to save un-necessary bandwidth usage
            if (messageStatsQueue.Count <= 100)
            {
                return;
            }

            var model = StatModel.Load();
            model.MessageStats.AddRange(messageStatsQueue);
            model.Save();

            // Reset the queue to ensure that the queue is never too long and messages aren't logged multiple times
            messageStatsQueue = new List<StatModel.MessageStat>();
            */
        }

        /// <summary>
        /// Creates a new command guild based on a guild Id
        /// </summary>
        /// <param name="guildId">
        /// The guild id.
        /// </param>
        /// <returns>
        /// The <see cref="StatModel.CommandStat.CommandGuild"/>.
        /// </returns>
        public static StatModel.CommandStat.CommandGuild NewCommandGuild(ulong guildId)
        {
            return new StatModel.CommandStat.CommandGuild { GuildId = guildId, Uses = 1 };
        }

        /// <summary>
        /// Creates a new command user
        /// </summary>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// The <see cref="StatModel.CommandStat.CommandUser"/>.
        /// </returns>
        public static StatModel.CommandStat.CommandUser NewCommandUser(ulong userId)
        {
            return new StatModel.CommandStat.CommandUser { UserId = userId, Uses = 1 };
        }
    }
}
