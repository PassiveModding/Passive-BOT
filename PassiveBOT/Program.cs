namespace PassiveBOT
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.PrefixService;
    using Discord.Commands;
    using Discord.WebSocket;

    using DiscordBotsList.Api.Adapter.Discord.Net;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    using PassiveBOT.Context;
    using PassiveBOT.Extensions;
    using PassiveBOT.Extensions.PassiveBOT;
    using PassiveBOT.Handlers;
    using PassiveBOT.Models;
    using PassiveBOT.Services;

    using Raven.Client.Documents;

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

            LogHandler.LogMessage("Loading initial provider", LogSeverity.Verbose);
            var services = new ServiceCollection()
                .AddSingleton<DatabaseHandler>()
                .AddSingleton(x => x.GetRequiredService<DatabaseHandler>().Execute<ConfigModel>(DatabaseHandler.Operation.LOAD, id: "Config"))
                .AddSingleton(new CommandService(new CommandServiceConfig
                            {
                                ThrowOnError = false,
                                IgnoreExtraArgs = false,
                                DefaultRunMode = RunMode.Async
                            }))
                .AddSingleton(
                    x =>
                        {
                            if (File.Exists(Path.Combine(AppContext.BaseDirectory, "setup/DBConfig.json")))
                            {
                                var Settings = JsonConvert.DeserializeObject<DatabaseObject>(File.ReadAllText("setup/DBConfig.json"));
                                return Settings;
                            }

                            return new DatabaseObject();
                        })
                .AddSingleton(x =>
                    {
                        if (File.Exists(Path.Combine(AppContext.BaseDirectory, "setup/DBConfig.json")))
                        {
                            var Settings = JsonConvert.DeserializeObject<DatabaseObject>(File.ReadAllText("setup/DBConfig.json"));
                            if (!string.IsNullOrEmpty(Settings.ProxyUrl))
                            {
                                return new HttpClientHandler { Proxy = new WebProxy(Settings.ProxyUrl), UseProxy = true };
                            }
                        }

                        return new HttpClientHandler();
                    })
                .AddSingleton(x => new HttpClient(x.GetRequiredService<HttpClientHandler>()))
                .AddSingleton<BotHandler>()
                .AddSingleton<EventHandler>()
                .AddSingleton<Events>()
                .AddSingleton(x => x.GetRequiredService<DatabaseHandler>().InitializeAsync().Result)
                .AddSingleton(new Random(Guid.NewGuid().GetHashCode()))
                .AddSingleton(x =>
                new DiscordShardedClient(
                    new DiscordSocketConfig
                        {
                            MessageCacheSize = 0,
                            AlwaysDownloadUsers = false,
                            LogLevel = LogSeverity.Info,
                            TotalShards = x.GetRequiredService<ConfigModel>().Shards
                        }))
                .AddSingleton(
                   x =>
                        {
                            var config = x.GetRequiredService<ConfigModel>().Prefix;
                            var store = x.GetRequiredService<IDocumentStore>();
                            return new PrefixService(config, store);
                        })
                .AddSingleton<TagService>()
                .AddSingleton<PartnerService>()
                .AddSingleton<LevelService>()
                .AddSingleton<ChannelService>()
                .AddSingleton<HomeService>()
                .AddSingleton<ChannelHelper>()
                .AddSingleton<PartnerHelper>()
                .AddSingleton<Interactive>()
                .AddSingleton(
                    x =>
                        {
                            var limits = new TranslateLimitsNew(x.GetRequiredService<IDocumentStore>());
                            limits.Initialize();
                            return limits;
                        })
                .AddSingleton(x => new TranslateMethodsNew(x.GetRequiredService<DatabaseObject>(), x.GetRequiredService<TranslateLimitsNew>(), x.GetRequiredService<ConfigModel>()))
                .AddSingleton<LevelHelper>()
                .AddSingleton<TranslationService>()
                .AddSingleton(
                    x =>
                        {
                            var birthdayService = new BirthdayService(x.GetRequiredService<DiscordShardedClient>(), x.GetRequiredService<IDocumentStore>());
                            birthdayService.Initialize();
                            return birthdayService;
                        })
                .AddSingleton(
                    x =>
                        {
                            var gameService = new GameService(x.GetRequiredService<IDocumentStore>(), x.GetRequiredService<DatabaseObject>());
                            gameService.Initialize();
                            return gameService;
                        })
                .AddSingleton<TimerService>()
                .AddSingleton<DBLApiService>()
                .AddSingleton<ReminderService>();

            var provider = services.BuildServiceProvider();
            
            LogHandler.LogMessage("Initializing HomeService", LogSeverity.Verbose);
            provider.GetRequiredService<HomeService>().Update();
            LogHandler.LogMessage("Initializing PrefixService", LogSeverity.Verbose);
            await provider.GetRequiredService<PrefixService>().InitializeAsync();
            LogHandler.LogMessage("Initializing BotHandler", LogSeverity.Verbose);
            await provider.GetRequiredService<BotHandler>().InitializeAsync();
            LogHandler.LogMessage("Initializing EventHandler", LogSeverity.Verbose);
            await provider.GetRequiredService<EventHandler>().InitializeAsync();


            // Indefinitely delay the method from finishing so that the program stays running until stopped.
            await Task.Delay(-1);
        }        
    }
}