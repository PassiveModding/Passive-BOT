namespace PassiveBOT.Handlers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.Addons.PrefixService;
    using global::Discord.Commands;

    using global::Discord.WebSocket;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    using PassiveBOT.Context;
    using PassiveBOT.Extensions;
    using PassiveBOT.Extensions.PassiveBOT;
    using PassiveBOT.Models;
    using PassiveBOT.Models.Migration;
    using PassiveBOT.TypeReaders;

    /// <summary>
    /// The event handler.
    /// </summary>
    public class EventHandler
    {
        /// <summary>
        /// true = will override all prefixes and read from DatabaseObject
        /// Useful for testing on the main bot account without a prefix conflict
        /// </summary>
        private readonly bool prefixOverride;

        /// <summary>
        /// Messages that have already been translated.
        /// </summary>
        private readonly ConcurrentDictionary<ulong, List<LanguageMap.LanguageCode>> translated = new ConcurrentDictionary<ulong, List<LanguageMap.LanguageCode>>();

        /// <summary>
        /// true = check and update all missing servers on start.
        /// </summary>
        private bool guildCheck = true;

        /// <summary>
        /// Displays bot invite on connection Once then gets toggled off.
        /// </summary>
        private bool hideInvite;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandler"/> class.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="service">
        /// The service.
        /// </param>
        /// <param name="levels">
        /// The levels.
        /// </param>
        /// <param name="channelHelper">
        /// The channel Helper.
        /// </param>
        /// <param name="commandService">
        /// The command service.
        /// </param>
        /// <param name="prefixService">
        /// The prefix Service.
        /// </param>
        public EventHandler(DiscordShardedClient client, ConfigModel config, IServiceProvider service, LevelHelper levels, ChannelHelper channelHelper, CommandService commandService, PrefixService prefixService)
        {
            Client = client;
            Config = config;
            Provider = service;
            CommandService = commandService;
            prefixOverride = DatabaseHandler.Settings.UsePrefixOverride;
            PrefixService = prefixService;
            _ChannelHelper = channelHelper;
            _LevelHelper = levels;

            CancellationToken = new CancellationTokenSource();
        }

        private LevelHelper _LevelHelper { get; }

        /// <summary>
        /// Gets the provider.
        /// </summary>
        private PrefixService PrefixService { get; }

        /// <summary>
        /// Gets the config.
        /// </summary>
        private ConfigModel Config { get; }

        private ChannelHelper _ChannelHelper { get; }

        /// <summary>
        /// Gets the provider.
        /// </summary>
        private IServiceProvider Provider { get; }

        /// <summary>
        /// Gets the client.
        /// </summary>
        private DiscordShardedClient Client { get; }

        /// <summary>
        /// Gets the command service.
        /// </summary>
        private CommandService CommandService { get; }

        /// <summary>
        /// Gets or sets the cancellation token.
        /// </summary>
        private CancellationTokenSource CancellationToken { get; set; }

        /// <summary>
        /// The initialize async.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task InitializeAsync()
        {
            // Ensure that the EmojiTypeReader is initialized so we can parse an emoji as a parameter
            CommandService.AddTypeReader(typeof(Emoji), new EmojiTypeReader());

            // This will add all our modules to the command service, allowing them to be accessed as necessary
            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly(), Provider);
            LogHandler.LogMessage("RavenBOT: Modules Added");
        }

        /// <summary>
        /// Triggers when a shard is ready
        /// </summary>
        /// <param name="socketClient">
        /// The socketClient.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        internal async Task ShardReadyAsync(DiscordSocketClient socketClient)
        {
            await socketClient.SetActivityAsync(new Game($"Shard: {socketClient.ShardId}", ActivityType.Watching));

            if (guildCheck)
            {
                if (Client.Shards.All(x => x.ConnectionState == ConnectionState.Connected))
                { 
                    if (prefixOverride)
                    {
                        LogHandler.LogMessage("Bot is in Prefix Override Mode!", LogSeverity.Warning);
                    }

                    _ = Task.Run(
                        () =>
                            {
                                var handler = Provider.GetRequiredService<DatabaseHandler>();

                                // Returns all stored guild models
                                var guildIds = Client.Guilds.Select(g => g.Id).ToList();
                                var missingList = handler.Query<GuildModel>().Where(g => !g.Settings.Config.SaveGuildModel).Select(x => x.ID).Where(x => !guildIds.Contains(x)).ToList();

                                foreach (var id in missingList)
                                {
                                    handler.Execute<GuildModel>(DatabaseHandler.Operation.DELETE, id: id.ToString());
                                    handler.Execute<GuildModel>(DatabaseHandler.Operation.DELETE, id: $"{id}-Tags");
                                    handler.Execute<GuildModel>(DatabaseHandler.Operation.DELETE, id: $"{id}-Channels");
                                    handler.Execute<GuildModel>(DatabaseHandler.Operation.DELETE, id: $"{id}-Levels");
                                }

                                missingList = null;
                            });

                    // Ensure that this is only run once as the bot initially connects.
                    guildCheck = false;
                }
                else
                {
                    // This will check to ensure that all our servers are initialized, whilst also allowing the bot to continue starting
                    _ = Task.Run(
                        () =>
                            {
                                var handler = Provider.GetRequiredService<DatabaseHandler>();

                                // This will load all guild models and retrieve their IDs
                                var Servers = handler.Query<GuildModel>();;
                                var ids = Servers.Select(s => s.ID).ToList();

                                // Now if the bots server list contains a guild but 'Servers' does not, we create a new object for the guild
                                foreach (var Guild in socketClient.Guilds.Select(x => x.Id))
                                {
                                    if (!ids.Contains(Guild))
                                    {
                                        handler.Execute<GuildModel>(DatabaseHandler.Operation.CREATE, new GuildModel(Guild), Guild);
                                    }
                                }

                                ids = null;
                                Servers = null;
                            });
                }
            }

            LogHandler.LogMessage($"Shard: {socketClient.ShardId} Ready");
            if (!hideInvite)
            {
                LogHandler.LogMessage($"Invite: https://discordapp.com/oauth2/authorize?client_id={Client.CurrentUser.Id}&scope=bot&permissions=2146958591");
                hideInvite = true;
            }
        }

        /// <summary>
        /// Triggers when a shard connects.
        /// </summary>
        /// <param name="socketClient">
        /// The Client.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        internal Task ShardConnectedAsync(DiscordSocketClient socketClient)
        {
            Task.Run(()
                => CancellationToken.Cancel()).ContinueWith(x
                => CancellationToken = new CancellationTokenSource());
            LogHandler.LogMessage($"Shard: {socketClient.ShardId} Connected with {socketClient.Guilds.Count} Guilds and {socketClient.Guilds.Sum(x => x.MemberCount)} Users");
            return Task.CompletedTask;
        }

        /// <summary>
        /// This logs discord messages to our LogHandler
        /// </summary>
        /// <param name="message">
        /// The Message.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        internal Task LogAsync(LogMessage message)
        {
            return Task.Run(() => LogHandler.LogMessage(message.Message, message.Severity));
        }

        /// <summary>
        /// This will auto-remove the bot from servers as it gets removed. NOTE: Remove this if you want to save configs.
        /// </summary>
        /// <param name="guild">
        /// The guild.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        internal Task LeftGuildAsync(SocketGuild guild)
        {
            return Task.Run(
                () =>
                    {
                        var handler = Provider.GetRequiredService<DatabaseHandler>();

                        // Load the guild model from our database
                        var model = handler.Execute<GuildModel>(DatabaseHandler.Operation.LOAD, null, guild.Id);

                        // Check for null to ensure the guild model exists
                        if (model == null)
                        {
                            return;
                        }

                        // Ensure that the server has NOT enabled the save guild model feature before deleting
                        if (!model.Settings.Config.SaveGuildModel)
                        {
                            // Delete the guild model
                            handler.Execute<GuildModel>(DatabaseHandler.Operation.DELETE, null, guild.Id);
                        }
                    });
        }

        /// <summary>
        /// Bod Joined guild event
        /// </summary>
        /// <param name="guild">
        /// The guild.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        internal async Task JoinedGuildAsync(SocketGuild guild)
        {
            var handler = Provider.GetRequiredService<DatabaseHandler>();
            if (handler.Execute<GuildModel>(DatabaseHandler.Operation.LOAD, id: guild.Id) == null)
            {
                handler.Execute<GuildModel>(DatabaseHandler.Operation.CREATE, new GuildModel(guild.Id), guild.Id);
            }

            var general = guild.TextChannels.FirstOrDefault(x => string.Equals(x.Name, "general", StringComparison.CurrentCultureIgnoreCase));
            if (general != null)
            {
                try
                {
                    await general.SendMessageAsync(string.Empty, false, new EmbedBuilder
                    {
                        Title = $"Hi I am {Client.CurrentUser.Username}!",
                        Description = "You can get started using my commands by looking at all my commands\n" +
                                                                                              $"Type `{Provider.GetRequiredService<ConfigModel>().Prefix}Help` For commands you can access\n" +
                                                                                              $"or type `{Provider.GetRequiredService<ConfigModel>().Prefix}FullHelp` For all commands",
                        Color = Color.DarkRed
                    }.Build());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// This event is triggered every time the a user sends a Message in a channel, dm etc. that the bot has access to view.
        /// </summary>
        /// <param name="socketMessage">
        /// The socket Message.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        internal async Task MessageReceivedAsync(SocketMessage socketMessage)
        {
            if (!(socketMessage is SocketUserMessage Message) || Message.Channel is IDMChannel)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Message.Content) || Message.Content.Length < 3)
            {
                return;
            }

            var context = new Context(Client, Message, Provider);

            if (Config.LogUserMessages)
            {
                // Log user messages if enabled.
                LogHandler.LogMessage(context);
            }

            if (context.User.IsBot)
            {
                // Filter out all bot messages from triggering commands.
                return;
            }

            await _ChannelHelper.DoMediaChannelAsync(context);

            var argPos = 0;
            var isPrefixed = true;
            
            if (prefixOverride)
            {
                var config = JsonConvert.DeserializeObject<DatabaseObject>(File.ReadAllText("setup/DBConfig.json"));
                if (config.PrefixOverride != null)
                {
                    if (!context.Message.HasStringPrefix(config.PrefixOverride, ref argPos))
                    {
                        isPrefixed = false;
                    }
                }
                else
                {
                    LogHandler.LogMessage("Message Handler is being returned as the bot is in prefix override mode and you haven't specified a custom prefix in DBConfig.json", LogSeverity.Warning);
                    isPrefixed = false;
                }
            }
            else
            {
                // Filter out all messages that don't start with our Bot PrefixSetup, bot mention or server specific PrefixSetup.
                if (!(context.Message.HasMentionPrefix(context.Client.CurrentUser, ref argPos) || context.Message.HasStringPrefix(PrefixService.GetPrefix(context.Guild?.Id ?? 0), ref argPos)))
                {
                    isPrefixed = false;
                }
            }

            // run level check and auto-message channel check if the current message is not a 
            if (!isPrefixed)
            {
                var messageTask = Task.Run(
                    async () =>
                        {
                            LogHandler.LogMessage("Running Message Tasks", LogSeverity.Verbose);
                            await _LevelHelper.DoLevelsAsync(context);
                            await _ChannelHelper.DoAutoMessageAsync(context);
                            StatHelper.LogMessage(context.Message);
                        });
                return;
            }

            // Ensure that blacklisted users/guilds are not allowed to run commands
            if (CheckBlacklist(Message.Author.Id, context.Guild.Id))
            {
                return;
            }

            // Here we attempt to execute a command based on the user Message
            var commandTask = Task.Run(async () =>
                {
                    var result = await CommandService.ExecuteAsync(context, argPos, Provider, MultiMatchHandling.Best);

                    // Generate an error Message for users if a command is unsuccessful
                    if (!result.IsSuccess)
                    {
                        await CmdErrorAsync(context, result, argPos);
                    }
                    else
                    {
                        var search = CommandService.Search(context, argPos);
                        var cmd = search.Commands.FirstOrDefault();
                        StatHelper.LogCommand(cmd.Command, Message);
                        if (Config.LogCommandUsages)
                        {
                            LogHandler.LogMessage(context);
                        }
                    }
                });
        }

        /// <summary>
        /// The check blacklist.
        /// </summary>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <param name="guildId">
        /// The guild Id.
        /// </param>
        /// <returns>
        /// Whether the user has been blacklisted in home model
        /// </returns>
        internal bool CheckBlacklist(ulong userId, ulong guildId)
        {
            var home = HomeModel.Load();
            if (home.Blacklist.BlacklistedUsers.Contains(userId))
            {
                return true;
            }

            if (home.Blacklist.BlacklistedGuilds.Contains(guildId))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// The _client_ reaction added.
        /// </summary>
        /// <param name="messageCacheable">
        /// The message Cache-able.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="reaction">
        /// The reaction.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        internal async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> messageCacheable, ISocketMessageChannel channel, SocketReaction reaction)
        {
            LogHandler.LogMessage("Reaction Detected", LogSeverity.Verbose);

            SocketUserMessage message;

            // SocketUser reactor;
            if (!reaction.Message.IsSpecified)
            {
                message = (await channel.GetMessageAsync(reaction.MessageId)) as SocketUserMessage;
            }
            else
            {
                message = reaction.Message.Value;
            }

            if (message == null)
            {
                return;
            }
            
            if (reaction.User.Value.IsBot || string.IsNullOrWhiteSpace(message.Content))
            {
                return;
            }
                
            /*
            if (!reaction.User.IsSpecified)
            {
                reactor = (channel as SocketGuildChannel).Guild.GetUser(reaction.UserId);
            }
            else
            {
                reactor = reaction.User.Value as SocketUser;
            }
            */
            
            try
            {
                var translateAction = Task.Run(
                    async () =>
                        {
                            var guild = Provider.GetRequiredService<DatabaseHandler>().Execute<GuildModel>(DatabaseHandler.Operation.LOAD, null, (channel as SocketGuildChannel).Guild.Id.ToString());
                            if (!guild.Settings.Translate.EasyTranslate)
                            {
                                return;
                            }

                            // Check custom matches first
                            var languageType = guild.Settings.Translate.CustomPairs.FirstOrDefault(x => x.EmoteMatches.Any(val => val == reaction.Emote.Name));

                            if (languageType == null)
                            {
                                // If no custom matches, check default matches
                                languageType = LanguageMap.DefaultMap.FirstOrDefault(x => x.EmoteMatches.Any(val => val == reaction.Emote.Name));
                                if (languageType == null)
                                {
                                    return;
                                }
                            }

                            if (translated.Any(x => x.Key == reaction.MessageId && x.Value.Contains(languageType.Language)))
                            {
                                LogHandler.LogMessage("Ignored EasyTranslate Reaction", LogSeverity.Verbose);
                                return;
                            }

                            var embed = await TranslateMethods.TranslateEmbedAsync(languageType.Language, Provider, message, reaction);

                            if (guild.Settings.Translate.DMTranslations)
                            {
                                try
                                {
                                    await reaction.User.Value.SendMessageAsync(string.Empty, false, embed.Build());
                                }
                                catch
                                {
                                    await reaction.Channel.SendMessageAsync($"Unable to send DM Translation to {reaction.User.Value?.Mention}");
                                }
                            }
                            else
                            {
                                await channel.SendMessageAsync(string.Empty, false, embed.Build());
                                var match = translated.FirstOrDefault(x => x.Key == reaction.MessageId);
                                if (match.Value == null)
                                {
                                    translated.TryAdd(reaction.MessageId, new List<LanguageMap.LanguageCode> { languageType.Language });
                                }
                                else
                                {
                                    match.Value.Add(languageType.Language);
                                }
                            }

                            LogHandler.LogMessage(guild.ID, reaction.Channel.Name, reaction.UserId, $"Translated Message to {languageType.Language}: {message.Content}");
                        });
            }
            catch (Exception e)
            {
                LogHandler.LogMessage(e.ToString(), LogSeverity.Error);
            }
        }

        /// <summary>
        /// Generates an error Message based on a command error.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <param name="argPos">
        /// The arg pos.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        internal async Task CmdErrorAsync(Context context, IResult result, int argPos)
        {
            string errorMessage;
            if (result.Error == CommandError.UnknownCommand)
            {
                errorMessage = "**Command:** N/A";
            }
            else
            {
                // Search the commandservice based on the Message, then respond accordingly with information about the command.
                var search = CommandService.Search(context, argPos);
                var cmd = search.Commands.FirstOrDefault();

                errorMessage = $"**Command Name:** `{cmd.Command.Name}`\n" +
                               $"**Summary:** `{cmd.Command?.Summary ?? "N/A"}`\n" +
                               $"**Remarks:** `{cmd.Command?.Remarks ?? "N/A"}`\n" +
                               $"**Aliases:** {(cmd.Command.Aliases.Any() ? string.Join(" ", cmd.Command.Aliases.Select(x => $"`{x}`")) : "N/A")}\n" +
                               $"**Parameters:** {(cmd.Command.Parameters.Any() ? string.Join(" ", cmd.Command.Parameters.Select(x => x.IsOptional ? $" `<(Optional){x.Name}>` " : $" `<{x.Name}>` ")) : "N/A")}\n" +
                               "**Error Reason**\n" +
                               $"{result.ErrorReason}";
                StatHelper.LogCommand(cmd.Command, context.Message, true);
            }

            try
            {
                await context.Channel.SendMessageAsync(string.Empty, false, new EmbedBuilder
                {
                    Title = "ERROR",
                    Description = errorMessage
                }.Build());
            }
            catch
            {
                // ignored
            }

            LogError(result, context);
        }

        /// <summary>
        /// Logs specified errors based on type.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        internal void LogError(IResult result, Context context)
        {
            switch (result.Error)
            {
                case CommandError.MultipleMatches:
                    if (Config.LogCommandUsages)
                    {
                        LogHandler.LogMessage(context, result.ErrorReason, LogSeverity.Error);
                    }

                    break;
                case CommandError.ObjectNotFound:
                    if (Config.LogCommandUsages)
                    {
                        LogHandler.LogMessage(context, result.ErrorReason, LogSeverity.Error);
                    }

                    break;
            }
        }
    }
}