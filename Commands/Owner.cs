using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;
using PassiveBOT.preconditions;

namespace PassiveBOT.Commands
{
    [RequireOwner]
    public class Owner : ModuleBase
    {
        public readonly CommandService Service;
        public DiscordSocketClient Client;

        public Owner(CommandService service)
        {
            Service = service;
        }

        [Command("PurgeServers+", RunMode = RunMode.Async)]
        [Summary("PurgeServers+")]
        [Remarks("Delete old server configs")]
        public async Task DeleteServerConfigs()
        {
            await ReplyAsync("Working....");
            var purged = 0;
            foreach (var config in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "setup/server/")))
            {
                var p = Path.GetFileNameWithoutExtension(config);
                //Console.WriteLine(p);
                try
                {
                    var trythis = ((DiscordSocketClient) Context.Client).GetGuild(Convert.ToUInt64(p));
                    Console.WriteLine(trythis.Name);
                }
                catch
                {
                    File.Delete(config);
                    purged++;
                }

            }
            await ReplyAsync("Guilds Purged.\n" +
                             $"Purged: {purged}");
        }



        [Command("help+", RunMode = RunMode.Async)]
        [Summary("help+")]
        [Remarks("Owner Commands")]
        [Ratelimit(1, 15, Measure.Seconds)]
        public async Task Help2Async()
        {
            var description = "";
            foreach (var module in Service.Modules)
                if (module.Name == "Owner")
                    description = module.Commands.Aggregate(description,
                        (current, cmd) => current + $"{Load.Pre}{cmd.Aliases.First()} - {cmd.Remarks}\n");

            var embed = new EmbedBuilder()
                .WithTitle("Owner Commands")
                .WithDescription(description);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("die+")]
        [Summary("die+")]
        [Remarks("Kills the bot")]
        public async Task Die()
        {
            await ReplyAsync("Bye Bye :heart:");
            Environment.Exit(0);
        }

        [Command("LeaveServer+")]
        [Summary("Leave+ <guild ID> [Optional]<reason>")]
        [Remarks("Makes the bot leave the specified guild")]
        public async Task LeaveAsync(ulong id, [Remainder] string reason = "No reason provided by the owner.")
        {
            if (id <= 0)
                await ReplyAsync("Please enter a valid Guild ID");
            var gld = await Context.Client.GetGuildAsync(id);
            var ch = await gld.GetDefaultChannelAsync();

            await ch.SendMessageAsync($"haha fuck this shit I'm out... `{reason}`");
            await Task.Delay(5000);
            await gld.LeaveAsync();
            await ReplyAsync("Message has been sent and I've left the guild!");
        }

        [Command("Username+")]
        [Summary("username+ <name>")]
        [Remarks("Sets the bots username")]
        public async Task UsernameAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                await ReplyAsync("Value cannot be empty");
            await Context.Client.CurrentUser.ModifyAsync(x => x.Username = value).ConfigureAwait(false);
            await ReplyAsync("Bot Username updated").ConfigureAwait(false);
        }
    }
}