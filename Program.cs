using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using PassiveBOT;
using PassiveBOT.Services;
using System.Linq;

namespace PassiveBOT
{
    public class Program
    {
        // Convert our sync main to an async main.
        public static void Main(string[] args) =>
         new Program().Start().GetAwaiter().GetResult();

        private DiscordSocketClient client;
        private CommandHandler handler;

        public async Task Start()
        {
            Console.WriteLine($"===   PassiveBOT  ===");
            Console.Title = $"PassiveBOT";

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
             "| Designed by PassiveModding PassiveNation.com/ ||    Status: Connected   | \n" +
             "\\--------------------------------------------------------------------------/ \n");

            var map = new DependencyMap();
            map.Add(client);

            handler = new CommandHandler();
            await handler.Install(map);


            client.Log += Log;
            client.Ready += Client_Ready;

            await Task.Delay(3000);
            while (true)
            {
                var rnd = new Random().Next(0, 4);
                if (rnd == 0)
                    await client.SetGameAsync($"{Config.Load().Prefix}help / Users: {(client as DiscordSocketClient).Guilds.Sum(g => g.MemberCount)}");
                else if (rnd == 1)
                    await client.SetGameAsync($"{Config.Load().Prefix}help / Servers: {(client as DiscordSocketClient).Guilds.Count}");
                else if (rnd == 2)
                    await client.SetGameAsync($"{Config.Load().Prefix}help / Heap: {GetHeapSize()}MB");
                else if (rnd == 3)
                    await client.SetGameAsync($"{Config.Load().Prefix}help / {Linkcfg.gamesite}");
                else if (rnd == 4)
                    await client.SetGameAsync($"{Config.Load().Prefix}help / <3");
                else
                    await client.SetGameAsync($"{Config.Load().Prefix}help / Users: {(client as DiscordSocketClient).Guilds.Sum(g => g.MemberCount)}");
                await Log(new LogMessage(LogSeverity.Info, "SetGame", $"SetGame         | Server: All Guilds      | Automated"));
                await Task.Delay(3600000);
                
            }
        }
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();

        private async Task Client_Ready()
        {
            var application = await client.GetApplicationInfoAsync();
            await Log(new LogMessage(LogSeverity.Info, "Program",
                $"Invite URL: < https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot >"));
        }

        public static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}