using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PassiveBOT.Configuration;
using PassiveBOT.Handlers;
using Color = System.Drawing.Color;
using EventHandler = PassiveBOT.Handlers.EventHandler;

namespace PassiveBOT
{
    public class Program
    {
        private CommandHandler _handler;
        public DiscordShardedClient Client;
        //public static List<string> Keys { get; set; }

        public static void Main(string[] args)
        {
            new Program().Start().GetAwaiter().GetResult();
        }

        public async Task Start()
        {
            Console.Title = $"PassiveBOT";
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

            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "setup/")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "setup/"));
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "setup/config/")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "setup/config/"));
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "setup/server/")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "setup/server/"));
            if (!File.Exists(Path.Combine(AppContext.BaseDirectory, "setup/config/home.json")))
                File.Create(Path.Combine(AppContext.BaseDirectory, "setup/config/home.json")).Dispose();
            Config.CheckExistence();
            var prefix = Config.Load().Prefix;
            var debug = Config.Load().Debug.ToUpper();
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
                    await ColourLog.In1Run($"Error Loading Debug Config, Set to default (Entry = {debug})");
                    break;
            }


            Client = new DiscordShardedClient(new DiscordSocketConfig
            {
                LogLevel = ll,
                //MessageCacheSize = 500
            });

            try
            {
                await Client.LoginAsync(TokenType.Bot, token);
                await Client.StartAsync();
            }
            catch (Exception e)
            {
                await ColourLog.In1Run("Token was rejected by Discord (Invalid Token or Connection Error)\n" +
                                       $"{e}");
            }

            var serviceProvider = ConfigureServices();
            _handler = new CommandHandler(serviceProvider);
            //var _ = new EventHandler(serviceProvider);
            await _handler.ConfigureAsync();

            //checks if the user wants to log debug info or not
            if (ll == LogSeverity.Debug)
                Client.Log += LogDebug;
            else
                Client.Log += LogMessageInfo;


            //setgame loop
            await Task.Delay(5000);
            await Client.SetGameAsync($"{prefix}help / {Load.Gamesite}");
            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(new InteractiveService(Client))
                .AddSingleton(new CommandService(
                    new CommandServiceConfig {CaseSensitiveCommands = false, ThrowOnError = false}));
            return services.BuildServiceProvider();
        }

        public static Task LogMessageInfo(LogMessage message)
        {
            var messagestr = message.ToString();
            if (message.ToString().StartsWith("Unknown OpCode (8)") ||
                message.ToString().Contains("VOICE_STATE_UPDATE"))
                return Task.CompletedTask;
            var msg = messagestr.Substring(21, messagestr.Length - 21);
            ColourLog.In2("PassiveBOT", '?', $"{msg}", Color.Chartreuse);
            return Task.CompletedTask;
        }

        public static Task LogDebug(LogMessage msg)
        {
            ColourLog.Debug(msg.ToString());
            return Task.CompletedTask;
        }
    }
}