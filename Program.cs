using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net.Providers.WS4Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PassiveBOT.Configuration;
using PassiveBOT.Handlers;
using PassiveBOT.Services;

namespace PassiveBOT
{
    public class Program
    {
        private DiscordSocketClient _client;
        private CommandHandler _handler;

        public static void Main(string[] args)
        {
            new Program().Start().GetAwaiter().GetResult();
        }

        public async Task Start()
        {
            Console.Title = $"PassiveBOT v{Load.Version}";
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
            var prefix = Config.Load().Prefix;
            var debug = Config.Load().Debug;
            var token = Config.Load().Token;


            var ll = LogSeverity.Info;
            if (debug == "y" || debug == "Y")
                ll = LogSeverity.Debug;
            else if (debug == "n" || debug == "N")
                ll = LogSeverity.Info;
            else
                await ColourLog.ColourError($"Error Loading Debug Config, Set to default (Entry = {debug})");


            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                WebSocketProvider = WS4NetProvider.Instance,
                LogLevel = ll
            });

            try
            {
                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();
            }
            catch
            {
                await ColourLog.ColourError("Token was rejected by Discord (Invalid Token)");
            }

            var serviceProvider = ConfigureServices();
            _handler = new CommandHandler(serviceProvider);
            await _handler.ConfigureAsync();

            //if logging is debug log as debug essentially
            _client.Ready += Client_Ready;
            await Task.Delay(1000);
            if (ll == LogSeverity.Debug)
                _client.Log += LogDebug;
            else
                _client.Log += LogMessageInfo;


            //setgame loop
            await Task.Delay(5000);
            string[] gametitle =
            {
                $"{prefix}help / Users: {_client.Guilds.Sum(g => g.MemberCount)}",
                $"{prefix}help / Servers: {_client.Guilds.Count}",
                $"{prefix}help / Heap: {GetHeapSize()}MB",
                $"{prefix}help / {Load.Gamesite}",
                $"{prefix}help / v{Load.Version}"
            };
            while (true)
            {
                var rnd = new Random();
                var result = rnd.Next(0, gametitle.Length);
                await _client.SetGameAsync($"{gametitle[result]}");
                await LogInfo($"SetGame         | Server: All Guilds      | {gametitle[result]}");
                await Task.Delay(3600000);
            }

        }

        private static string GetHeapSize()
        {
            return Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.InvariantCulture);
        }


        private async Task Client_Ready()
        {
            var application = await _client.GetApplicationInfoAsync();
            await LogInfo($"Link: https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot");
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(new CommandService(
                    new CommandServiceConfig {CaseSensitiveCommands = false, ThrowOnError = false}))
                .AddPaginator(_client);
            return services.BuildServiceProvider();
        }

        public static Task LogInfoTrim(string message)
        {
            var msg = message.Substring(21, message.Length - 21);
            ColourLog.ColourInfo($"{msg}");
            return Task.CompletedTask;
        }

        public static Task LogInfo(string msg)
        {
            ColourLog.ColourInfo($"{msg}");
            return Task.CompletedTask;
        }

        public static Task LogMessageInfo(LogMessage message)
        {
            var messagestr = message.ToString();
            var msg = messagestr.Substring(21, messagestr.Length - 21);
            ColourLog.ColourInfo($"{msg}");
            return Task.CompletedTask;
        }

        public static Task LogDebug(LogMessage msg)
        {
            ColourLog.ColourDebug(msg.ToString());
            return Task.CompletedTask;
        }
    }
}