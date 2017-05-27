using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PassiveBOT.Configuration;
using PassiveBOT.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PassiveBOT
{
    public class Program
    {
        public static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandHandler _handler;

        public async Task Start()
        {
            Console.Title = $"PassiveBOT v{Load.version}";
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



            Config.CheckExistence();
            string prefix = Config.Load().Prefix;
            string debug = Config.Load().Debug;
            string token = Config.Load().Token;


            //broken
            var ll = LogSeverity.Info;
            if (debug == "y" || debug == "Y")
                ll = LogSeverity.Debug;
            else if (debug == "n" || debug == "N")
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
                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();
            }
            catch
            {
                await Handlers.LogHandler.LogErrorAsync($"Token was rejected by Discord", $"Invalid Token");
            }

            var serviceProvider = ConfigureServices();
            _handler = new CommandHandler(serviceProvider);
            await _handler.ConfigureAsync();

            //if logging is debug log as debug essentially
            _client.Ready += Client_Ready;
            if (ll == LogSeverity.Debug)
                _client.Log += LogClient;
            else
                _client.Log += LogCinfo;


            //setgame loop
            await Task.Delay(3000);
            string[] gametitle =
            {
                $"{prefix}help / Users: {(_client as DiscordSocketClient).Guilds.Sum(g => g.MemberCount)}",
                $"{prefix}help / Servers: {(_client as DiscordSocketClient).Guilds.Count}",
                $"{prefix}help / Heap: {GetHeapSize()}MB",
                $"{prefix}help / {Load.gamesite}",
                $"{prefix}help / v{Load.version}"
            };
            while (true)
            {
                var rnd = new Random();
                var result = rnd.Next(0, gametitle.Length);
                await _client.SetGameAsync($"{gametitle[result]}");
                await Logged($"SetGame         | Server: All Guilds      | {gametitle[result]}");
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
            var message = msg.ToString();
            var _message = message.Substring(21, message.Length - 21);
            Handlers.LogHandler.LogAsync(_message.ToString());
            return Task.CompletedTask;
        }

        public static Task LogClient(LogMessage msg)
        {
            Handlers.LogHandler.LogClientAsync(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
