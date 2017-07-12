using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PassiveBOT.Configuration;
using PassiveBOT.Handlers;
using PassiveBOT.Services;
using Color = System.Drawing.Color;

namespace PassiveBOT
{
    public class Program
    {
        private CommandHandler _handler;
        public DiscordSocketClient Client;

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

            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "setup/moderation/")))
            {
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "setup/moderation/"));
                if (!File.Exists($"{AppContext.BaseDirectory}setup/moderation/nopre.txt"))
                    File.Create($"{AppContext.BaseDirectory}setup/moderation/nopre.txt").Close();
                if (!File.Exists($"{AppContext.BaseDirectory}setup/moderation/errlogging.txt"))
                    File.Create($"{AppContext.BaseDirectory}setup/moderation/errlogging.txt").Close();
            }
            Config.CheckExistence();
            var prefix = Config.Load().Prefix;
            var debug = Config.Load().Debug;
            var token = Config.Load().Token;


            var ll = LogSeverity.Info;
            switch (debug)
            {
                case "Y":
                    ll = LogSeverity.Debug;
                    break;
                case "N":
                    ll = LogSeverity.Info;
                    break;
                default:
                    await ColourLog.ColourError($"Error Loading Debug Config, Set to default (Entry = {debug})");
                    break;
            }


            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = ll
            });

            try
            {
                await Client.LoginAsync(TokenType.Bot, token);
                await Client.StartAsync();
            }
            catch
            {
                await ColourLog.ColourError("Token was rejected by Discord (Invalid Token or Connection Error)");
            }

            var serviceProvider = ConfigureServices();
            _handler = new CommandHandler(serviceProvider);
            await _handler.ConfigureAsync();

            //checks if the user wants to log debug info or not
            if (ll == LogSeverity.Debug)
                Client.Log += LogDebug;
            else
                Client.Log += LogMessageInfo;

            Client.Ready += Client_Ready;

            //setgame loop
            await Task.Delay(5000);
            string[] gametitle =
            {
                $"{prefix}help / Heap: {GetHeapSize()}MB",
                $"{prefix}help / {Load.Gamesite}",
                $"{prefix}help / v{Load.Version}"
            };
            while (true)
            {
                var rnd = new Random();
                var result = rnd.Next(0, gametitle.Length);
                await Client.SetGameAsync($"{gametitle[result]}");
                await LogInfo($"PassiveBOT      | SetGame                 | {gametitle[result]}");
                await Task.Delay(3600000);
            }
        }

        private static string GetHeapSize()
        {
            return Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.InvariantCulture);
        }


        private async Task Client_Ready()
        {
            var application = await Client.GetApplicationInfoAsync();
            await LogInfo(
                $"PassiveBOT      | Link: https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot&permissions=2146958591");
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(new AudioService())
                .AddSingleton(new CommandService(
                    new CommandServiceConfig {CaseSensitiveCommands = false, ThrowOnError = false}));
            return services.BuildServiceProvider();
        }

        public static Task LogInfo(string msg)
        {
            ColourLog.ColourInput($"{msg}", Color.Chartreuse);
            return Task.CompletedTask;
        }

        public static Task LogMessageInfo(LogMessage message)
        {
            var messagestr = message.ToString();
            if (message.ToString() == "Disconnecting" || message.ToString() == "Connecting" ||
                message.ToString().StartsWith("Unknown OpCode (8)") || message.ToString() == "Connected")
            {
            }
            var msg = messagestr.Substring(21, messagestr.Length - 21);
            msg = $"PassiveBOT      | {msg}";
            ColourLog.ColourInput($"{msg}", Color.Chartreuse);
            return Task.CompletedTask;
        }

        public static Task LogDebug(LogMessage msg)
        {
            ColourLog.ColourDebug(msg.ToString());
            return Task.CompletedTask;
        }
    }
}