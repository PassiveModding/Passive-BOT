using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using PassiveBOT.Configuration;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using PassiveBOT.Services;

namespace PassiveBOT
{
    public class Program
    {
        public static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandHandler _handler;

        public async Task Start()
        {
            Console.Title = $"PassiveBOT v{Linkcfg.version}";
            Console.WriteLine(
             "██████╗  █████╗ ███████╗███████╗██╗██╗   ██╗███████╗██████╗  ██████╗ ████████╗\n" +
             "██╔══██╗██╔══██╗██╔════╝██╔════╝██║██║   ██║██╔════╝██╔══██╗██╔═══██╗╚══██╔══╝\n" +
             "██████╔╝███████║███████╗███████╗██║██║   ██║█████╗  ██████╔╝██║   ██║   ██║   \n" +
             "██╔═══╝ ██╔══██║╚════██║╚════██║██║╚██╗ ██╔╝██╔══╝  ██╔══██╗██║   ██║   ██║   \n" +
             "██║     ██║  ██║███████║███████║██║ ╚████╔╝ ███████╗██████╔╝╚██████╔╝   ██║   \n" +
             "╚═╝     ╚═╝  ╚═╝╚══════╝╚══════╝╚═╝  ╚═══╝  ╚══════╝╚═════╝  ╚═════╝    ╚═╝   \n" +
             "/--------------------------------------------------------------------------\\ \n" +
             "| Designed by PassiveModding - PassiveNation.com  ||   Status: Connected   | \n" +
             "\\--------------------------------------------------------------------------/ \n");


            var ll = LogSeverity.Info;
            Config.CheckExistence();
            if (Config.Load().Debug == "y" || Config.Load().Debug == "Y")
                ll = LogSeverity.Debug;
            else if (Config.Load().Debug == "n" || Config.Load().Debug == "N")
                ll = LogSeverity.Info;
            else
                await Handlers.LogHandler.LogErrorAsync($"Error Loading Debug Config, Set to default", $"Info");

            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance,
                LogLevel = ll
            });

            try
            {
                await _client.LoginAsync(TokenType.Bot, Config.Load().Token);
                await _client.StartAsync();
            }
            catch
            {
                await Handlers.LogHandler.LogErrorAsync($"Token was rejected by Discord", $"Invalid Token");
            }

            var serviceProvider = ConfigureServices();
            _handler = new CommandHandler(serviceProvider);
            await _handler.ConfigureAsync();


            _client.Ready += Client_Ready;
            if (ll == LogSeverity.Debug)
                _client.Log += LogClient;
            else
                _client.Log += LogCinfo;

            //setgame loop
            await Task.Delay(3000);
            while (true)
            {
                var rnd = new Random().Next(0, 5);
                if (rnd == 0)
                {
                    var g0 = $"{Config.Load().Prefix}help / Users: {(_client as DiscordSocketClient).Guilds.Sum(g => g.MemberCount)}";
                    await _client.SetGameAsync($"{g0}");
                    await Logged($"SetGame         | Server: All Guilds      | {g0}");
                }
                else if (rnd == 1)
                {
                    var g1 = $"{Config.Load().Prefix}help / Servers: {(_client as DiscordSocketClient).Guilds.Count}";
                    await _client.SetGameAsync($"{g1}");
                    await Logged($"SetGame         | Server: All Guilds      | {g1}");
                }
                else if (rnd == 2)
                {
                    var g2 = $"{Config.Load().Prefix}help / Heap: {GetHeapSize()}MB";
                    await _client.SetGameAsync($"{g2}");
                    await Logged($"SetGame         | Server: All Guilds      | {g2}");
                }
                else if (rnd == 3)
                {
                    var g3 = $"{Config.Load().Prefix}help / {Linkcfg.gamesite}";
                    await _client.SetGameAsync($"{g3}");
                    await Logged($"SetGame         | Server: All Guilds      | {g3}");
                }
                else if (rnd == 4)
                {
                    var g4 = $"{Config.Load().Prefix}help / v{Linkcfg.version}";
                    await _client.SetGameAsync($"{g4}");
                    await Logged($"SetGame         | Server: All Guilds      | {g4}");
                }
                await Task.Delay(3600000);
            }
        }
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();

        private async Task Client_Ready()
        {
            var application = await _client.GetApplicationInfoAsync();
            await Logged($"Link: https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot");
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, ThrowOnError = false }))
                .AddPaginator(_client);
            return services.BuildServiceProvider();
        }

        public static Task Log(string message)
        {
            var msg = message.Substring(21, message.Length - 21);
            Handlers.LogHandler.LogAsync(msg.ToString());
            return Task.CompletedTask;
        }

        public static Task Logged(string msg)
        {
            Handlers.LogHandler.LogAsync(msg.ToString());
            return Task.CompletedTask;
        }

        public static Task LogCinfo(LogMessage msg)
        {
            Handlers.LogHandler.LogAsync(msg.ToString());
            return Task.CompletedTask;
        }

        public static Task LogClient(LogMessage msg)
        {
            Handlers.LogHandler.LogClientAsync(msg.ToString());
            return Task.CompletedTask;
        }
    }
}