namespace PassiveBOT
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.PrefixService;
    using Discord.Commands;
    using Discord.WebSocket;

    using Microsoft.Extensions.DependencyInjection;

    using PassiveBOT.Context;
    using PassiveBOT.Extensions.PassiveBOT;
    using PassiveBOT.Handlers;
    using PassiveBOT.Models;
    using PassiveBOT.Models.Migration;
    using PassiveBOT.Services;

    using EventHandler = PassiveBOT.Handlers.EventHandler;

    /// <summary>
    ///     The program.
    /// </summary>
    public class Program
    {
        /// <summary>
        ///     Gets or sets The client.
        /// </summary>
        public static DiscordShardedClient Client { get; set; }

        /// <summary>
        ///     Entry point of the program
        /// </summary>
        /// <param name="args">Discarded Args</param>
        public static void Main(string[] args)
        {
            StartAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Initialization of our service provider and bot
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public static async Task StartAsync()
        {
            // This ensures that our bots setup directory is initialized and will be were the database config is stored.
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "setup/")))
            {
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "setup/"));
            }

            var services = new ServiceCollection().AddSingleton<DatabaseHandler>().AddSingleton(x => x.GetRequiredService<DatabaseHandler>().Execute<ConfigModel>(DatabaseHandler.Operation.LOAD, id: "Config")).AddSingleton(x => x.GetRequiredService<DatabaseHandler>().Execute<HomeModel>(DatabaseHandler.Operation.LOAD, id: "HomeServer")).AddSingleton(new CommandService(new CommandServiceConfig { ThrowOnError = false, IgnoreExtraArgs = false, DefaultRunMode = RunMode.Sync })).AddSingleton(new HttpClient()).AddSingleton<BotHandler>().AddSingleton<EventHandler>().AddSingleton<Events>().AddSingleton(new Random(Guid.NewGuid().GetHashCode()));

            var provider = services.BuildServiceProvider();

            // This method ensures that the database is
            // 1. Running
            // 2. Set up properly 
            // 3. contains the bot config itself
            var store = await provider.GetRequiredService<DatabaseHandler>().InitializeAsync();

            // The provider is split here so we can get our shard count from the database before actually logging into discord.
            // This is important to do so the bot always logs in with the required amount of shards.
            var config = provider.GetRequiredService<DatabaseHandler>().Execute<ConfigModel>(DatabaseHandler.Operation.LOAD, id: "Config");

            services.AddSingleton(
                new DiscordShardedClient(
                    new DiscordSocketConfig
                        {
                            MessageCacheSize = 20,
                            AlwaysDownloadUsers = false,
                            LogLevel = LogSeverity.Info,

                            // Please change increase this as your server count grows beyond 2000 guilds. ie. < 2000 = 1, 2000 = 2, 4000 = 2 ...
                            TotalShards = config.Shards
                        })).AddSingleton(new PrefixService(config.Prefix, store))
                .AddSingleton(new TagService(store))
                .AddSingleton(new PartnerService(store))
                .AddSingleton(new LevelService(store))
                .AddSingleton(new ChannelService(store))
                .AddSingleton<ChannelHelper>()
                .AddSingleton<Interactive>()
                .AddSingleton<LevelHelper>()
                // .AddSingleton<GuildModelToServices>()
                .AddSingleton<TimerService>();

            // Build the service provider a second time so that the ShardedClient is now included.
            provider = services.BuildServiceProvider();

            await provider.GetRequiredService<PrefixService>().InitializeAsync();
            await provider.GetRequiredService<BotHandler>().InitializeAsync();
            await provider.GetRequiredService<EventHandler>().InitializeAsync();

            // provider.GetRequiredService<TimerService>().Restart();

            // Indefinitely delay the method from finishing so that the program stays running until stopped.
            await Task.Delay(-1);
        }
    }
}