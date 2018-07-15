namespace PassiveBOT.Handlers
{
    using System;
    using System.Threading.Tasks;

    using Discord;

    using Discord.WebSocket;

    using Microsoft.Extensions.DependencyInjection;

    using PassiveBOT.Extensions.PassiveBOT;
    using PassiveBOT.Models;

    /// <summary>
    /// The bot handler.
    /// </summary>
    public class BotHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotHandler"/> class.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="events">
        /// The EventSetup.
        /// </param>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public BotHandler(DiscordShardedClient client, EventHandler events, ConfigModel config, IServiceProvider provider)
        {
            Client = client;
            Event = events;
            Config = config;
            Provider = provider;
        }

        /// <summary>
        /// Gets the provider.
        /// </summary>
        private IServiceProvider Provider { get; }

        /// <summary>
        /// Gets the config.
        /// </summary>
        private ConfigModel Config { get; }

        /// <summary>
        /// Gets the event.
        /// </summary>
        private EventHandler Event { get; }

        /// <summary>
        /// Gets the client.
        /// </summary>
        private DiscordShardedClient Client { get; }

        /// <summary>
        /// Initializes and logs the bot in.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task InitializeAsync()
        {
            // These are our EventSetup, each time one of these is triggered it runs the corresponding method. Ie, the bot receives a PartnerMessage we run Event.MessageReceivedAsync
            Client.Log += Event.LogAsync;
            Client.ShardReady += Event.ShardReadyAsync;
            Client.LeftGuild += Event.LeftGuildAsync;
            Client.JoinedGuild += Event.JoinedGuildAsync;
            Client.ShardConnected += Event.ShardConnectedAsync;
            Client.ReactionAdded += Event.ReactionAddedAsync;
            Client.MessageReceived += Event.MessageReceivedAsync;
            Client.UserJoined += user => Events.UserJoinedAsync(Provider.GetRequiredService<DatabaseHandler>().Execute<GuildModel>(DatabaseHandler.Operation.LOAD, null, user.Guild.Id), user);
            Client.UserLeft += user => Events.UserLeftAsync(Provider.GetRequiredService<DatabaseHandler>().Execute<GuildModel>(DatabaseHandler.Operation.LOAD, null, user.Guild.Id), user);

            // Here we log the bot in and start it. This MUST run for the bot to connect to discord.
            await Client.LoginAsync(TokenType.Bot, Config.Token);
            LogHandler.LogMessage("=> Logged In");
            await Client.StartAsync();
            LogHandler.LogMessage("=> Started");
        }
    }
}