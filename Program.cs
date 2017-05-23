using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using PassiveBOT;
using PassiveBOT.Services;
using System.Linq;
using Discord.Addons.InteractiveCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;

namespace PassiveBOT
{
    public class Program
    {
        public static void Main(string[] args) =>
         new Program().Start().GetAwaiter().GetResult();

        private DiscordSocketClient client;
        private CommandHandler handler;

        public async Task Start()
        {
            Console.Title = $"PassiveBOT v{Linkcfg.version}";

            Config.CheckExistence();

            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance,
                LogLevel = LogSeverity.Info
            });

            await client.LoginAsync(TokenType.Bot, Config.Load().Token);
            await client.StartAsync();

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

            var map = new DependencyMap();
            map.Add(client);
            handler = new CommandHandler();
            await handler.Install(map);

            Console.WriteLine();
            client.Log += Log;
            client.Ready += Client_Ready;

            await Task.Delay(3000);
            while (true)
            {
                var rnd = new Random().Next(0, 5);
                if (rnd == 0)
                {
                    var g0 = $"{Config.Load().Prefix}help / Users: {(client as DiscordSocketClient).Guilds.Sum(g => g.MemberCount)}";
                    await client.SetGameAsync($"{g0}");
                    await Log(new LogMessage(LogSeverity.Info, "SetGame", $"SetGame         | Server: All Guilds      | {g0}"));
                }
                else if (rnd == 1)
                {
                    var g1 = $"{Config.Load().Prefix}help / Servers: {(client as DiscordSocketClient).Guilds.Count}";
                    await client.SetGameAsync($"{g1}");
                    await Log(new LogMessage(LogSeverity.Info, "SetGame", $"SetGame         | Server: All Guilds      | {g1}"));
                }
                else if (rnd == 2)
                {
                    var g2 = $"{Config.Load().Prefix}help / Heap: {GetHeapSize()}MB";
                    await client.SetGameAsync($"{g2}");
                    await Log(new LogMessage(LogSeverity.Info, "SetGame", $"SetGame         | Server: All Guilds      | {g2}"));
                }
                else if (rnd == 3)
                {
                    var g3 = $"{Config.Load().Prefix}help / {Linkcfg.gamesite}";
                    await client.SetGameAsync($"{g3}");
                    await Log(new LogMessage(LogSeverity.Info, "SetGame", $"SetGame         | Server: All Guilds      | {g3}"));
                }
                else if (rnd == 4)
                {
                    var g4 = $"{Config.Load().Prefix}help / v{Linkcfg.version}";
                    await client.SetGameAsync($"{g4}");
                    await Log(new LogMessage(LogSeverity.Info, "SetGame", $"SetGame         | Server: All Guilds      | {g4}"));
                }
                await Task.Delay(3600000);
            }
        }
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();

        private async Task Client_Ready()
        {
            var application = await client.GetApplicationInfoAsync();
            await Log(new LogMessage(LogSeverity.Info, "Program", $"Invite URL: https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot"));
        }

        public static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}