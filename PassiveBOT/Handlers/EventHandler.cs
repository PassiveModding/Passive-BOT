namespace PassiveBOT.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using global::Discord;

    using global::Discord.Commands;

    using global::Discord.WebSocket;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    using PassiveBOT.Discord.Context;
    using PassiveBOT.Discord.Extensions;
    using PassiveBOT.Discord.Extensions.PassiveBOT;
    using PassiveBOT.Discord.TypeReaders;
    using PassiveBOT.Models;

    /// <summary>
    /// The event handler.
    /// </summary>
    public class EventHandler
    {
        /// <summary>
        /// Messages that have already been translated.
        /// </summary>
        private readonly Dictionary<ulong, List<LanguageMap.LanguageCode>> translated = new Dictionary<ulong, List<LanguageMap.LanguageCode>>();

        /// <summary>
        /// This indicates how many shards have connected on initial bot use
        /// </summary>
        private readonly List<int> shardCheck = new List<int>();

        /// <summary>
        /// true = check and update all missing servers on start.
        /// </summary>
        private bool guildCheck = true;

        /// <summary>
        /// true = will override all prefixes and read from DatabaseObject
        /// Useful for testing on the main bot account without a prefix conflict
        /// </summary>
        private bool prefixOverride = true;

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
        /// <param name="commandService">
        /// The command service.
        /// </param>
        public EventHandler(DiscordShardedClient client, ConfigModel config, IServiceProvider service, CommandService commandService)
        {
            Client = client;
            Config = config;
            Provider = service;
            CommandService = commandService;

            CancellationToken = new CancellationTokenSource();
        }

        /// <summary>
        /// Gets the config.
        /// </summary>
        private ConfigModel Config { get; }

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
            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly());
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
        internal async Task ShardReady(DiscordSocketClient socketClient)
        {
            await socketClient.SetActivityAsync(new Game($"Shard: {socketClient.ShardId}", ActivityType.Watching));

            /*
            //Here we select at random out 'playing' Message.
             var Games = new Dictionary<ActivityType, string[]>
            {
                {ActivityType.Listening, new[]{"YT/PassiveModding", "Tech N9ne"} },
                {ActivityType.Playing, new[]{$"{Config.Prefix}help"} },
                {ActivityType.Watching, new []{"YT/PassiveModding"} }
            };
            var RandomActivity = Games.Keys.ToList()[Random.Next(Games.Keys.Count)];
            var RandomName = Games[RandomActivity][Random.Next(Games[RandomActivity].Length)];
            await socketClient.SetActivityAsync(new Game(RandomName, RandomActivity));
            LogHandler.LogMessage($"Game has been set to: [{RandomActivity}] {RandomName}");
            Games.Clear();
            */
            if (guildCheck)
            {
                if (shardCheck.Count < Client.Shards.Count)
                {
                    // This will check to ensure that all our servers are initialized, whilst also allowing the bot to continue starting
                    _ = Task.Run(
                        () =>
                            {
                                var handler = Provider.GetRequiredService<DatabaseHandler>();

                                // This will load all guild models and retrieve their IDs
                                var Servers = handler.Query<GuildModel>().Select(x => x.ID).ToList();

                                // Now if the bots server list contains a guild but 'Servers' does not, we create a new object for the guild
                                foreach (var Guild in socketClient.Guilds.Select(x => x.Id))
                                {
                                    if (!Servers.Contains(Guild))
                                    {
                                        handler.Execute<GuildModel>(DatabaseHandler.Operation.CREATE, new GuildModel { ID = Guild }, Guild);
                                    }
                                }

                                shardCheck.Add(socketClient.ShardId);
                            });
                }

                // Client.Shards.Select(x => x.ShardId).OrderByDescending(x => x).ToList() == shardCheck.Distinct().OrderByDescending(x => x).ToList()
                if (shardCheck.Count == Client.Shards.Count && Client.Shards.Select(x => x.ShardId).OrderByDescending(x => x).ToList().SequenceEqual(shardCheck.Distinct().ToList()))
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
                                var missingList = handler.Query<GuildModel>().Select(x => x.ID).Where(x => !guildIds.Contains(x)).ToList();

                                foreach (var id in missingList)
                                {
                                    handler.Execute<GuildModel>(DatabaseHandler.Operation.DELETE, id: id.ToString());
                                }
                            });

                    // Ensure that this is only run once as the bot initially connects.
                    guildCheck = false;
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
        internal Task ShardConnected(DiscordSocketClient socketClient)
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
        internal Task Log(LogMessage message)
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
        internal Task LeftGuild(SocketGuild guild)
        {
            return Task.Run(()
                => Provider.GetRequiredService<DatabaseHandler>().Execute<GuildModel>(DatabaseHandler.Operation.DELETE, id: guild.Id));
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
        internal async Task JoinedGuild(SocketGuild guild)
        {
            var handler = Provider.GetRequiredService<DatabaseHandler>();
            if (handler.Execute<GuildModel>(DatabaseHandler.Operation.LOAD, id: guild.Id) == null)
            {
                handler.Execute<GuildModel>(DatabaseHandler.Operation.CREATE, new GuildModel { ID = guild.Id }, guild.Id);
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

            await ChannelHelper.DoMediaChannel(context);

            var argPos = 0;
            bool isPrefixed = true;
            
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
                if (!(

                         // If the message starts with @BOTNAME and the server has bot @'s toggled on        
                         (context.Message.HasMentionPrefix(context.Client.CurrentUser, ref argPos) && !context.Server.Settings.Prefix.DenyMentionPrefix) ||

                         // If the message starts with the default bot prefix and it is toggled on
                         (context.Message.HasStringPrefix(Config.Prefix, ref argPos) && !context.Server.Settings.Prefix.DenyDefaultPrefix) ||

                         // If the message starts with the custom server prefix and the custom server prefix is set.
                         (context.Server.Settings.Prefix.CustomPrefix != null && context.Message.HasStringPrefix(context.Server.Settings.Prefix.CustomPrefix, ref argPos))))
                {
                    if (context.Message.HasStringPrefix(Config.Prefix, ref argPos) && context.Server.Settings.Prefix.DenyDefaultPrefix)
                    {
                        if (context.Server.Settings.Prefix.CustomPrefix != null)
                        {
                            // Ensure that if for some reason the server's custom prefix isn't set and but they are denying the default prefix that commands are still allowed
                            isPrefixed = false;
                        }
                    }
                    else
                    {
                        isPrefixed = false;
                    }
                }
            }

            // run level check and auto-message channel check if the current message is not a 
            if (!isPrefixed)
            {
                var messageTask = Task.Run(
                    async () =>
                        {
                            LogHandler.LogMessage("Running Message Tasks", LogSeverity.Verbose);
                            context = await LevelHelper.DoLevels(context);
                            await ChannelHelper.DoAutoMessage(context);
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
                        await CmdError(context, result, argPos);
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
        internal async Task ReactionAdded(Cacheable<IUserMessage, ulong> messageCacheable, ISocketMessageChannel channel, SocketReaction reaction)
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
                if (reaction.User.Value.IsBot || string.IsNullOrWhiteSpace(message.Content))
                {
                    return;
                }

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

                var embed = await TranslateMethods.TranslateEmbed(languageType.Language, Provider, message, reaction);

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
                        translated.Add(reaction.MessageId, new List<LanguageMap.LanguageCode>
                                                               {
                                                                   languageType.Language
                                                               });
                    }
                    else
                    {
                        match.Value.Add(languageType.Language);
                    }
                }

                LogHandler.LogMessage(guild.ID, reaction.Channel.Name, reaction.UserId, $"Translated Message to {languageType.Language}: {message.Content}");
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
        internal async Task CmdError(Context context, IResult result, int argPos)
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