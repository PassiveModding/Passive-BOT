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
            await Client.StopAsync();
            Environment.Exit(1);
        }

        [Command("Reconnect+")]
        [Summary("Reconnect+")]
        [Remarks("When you dont wanna kill me")]
        public async Task ReconnectAsync()
        {
            var client = Context.Client as DiscordSocketClient;
            await client.StopAsync();
            await Task.Delay(1000);
            await client.StartAsync();
            await ReplyAsync("Restarted!");
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

        [Command("nopre+")]
        [Summary("nopre+")]
        [Remarks("toggles prefixless commands in the current server")]
        [RequireContext(ContextType.Guild)]
        public async Task Nopre()
        {
            var lines = File.ReadAllLines(AppContext.BaseDirectory + "setup/moderation/nopre.txt");
            var result = lines.ToList();
            if (result.Contains(Context.Guild.Id.ToString()))
            {
                var oldLines = File.ReadAllLines($"{AppContext.BaseDirectory + "setup/moderation/nopre.txt"}");
                var newLines = oldLines.Where(line => !line.Contains(Context.Guild.Id.ToString()));
                File.WriteAllLines($"{AppContext.BaseDirectory + "setup/moderation/nopre.txt"}", newLines);
                await ReplyAsync(
                    $"{Context.Guild} has been removed from the noprefix list (secret commands and prefixless commands are now enabled)");
            }
            else
            {
                File.AppendAllText($"{AppContext.BaseDirectory + "setup/moderation/nopre.txt"}",
                    $"{Context.Guild.Id}" + Environment.NewLine);
                await ReplyAsync(
                    $"{Context.Guild} has been added to the noprefix list (secret commands and prefixless commands are now disabled)");
            }
        }

        [Command("errors+")]
        [Summary("errors+")]
        [Remarks("toggles error replies for this bot")]
        [RequireContext(ContextType.Guild)]
        public async Task ErrorLog()
        {
            var lines = File.ReadAllLines(AppContext.BaseDirectory + "setup/moderation/errlogging.txt");
            var result = lines.ToList();
            if (result.Contains(Context.Guild.Id.ToString()))
            {
                var oldLines = File.ReadAllLines($"{AppContext.BaseDirectory + "setup/moderation/errlogging.txt"}");
                var newLines = oldLines.Where(line => !line.Contains(Context.Guild.Id.ToString()));
                File.WriteAllLines($"{AppContext.BaseDirectory + "setup/moderation/errlogging.txt"}", newLines);
                await ReplyAsync($"I will no longer reply if an error is thrown in {Context.Guild}");
            }
            else
            {
                File.AppendAllText($"{AppContext.BaseDirectory + "setup/moderation/errlogging.txt"}",
                    $"{Context.Guild.Id}" + Environment.NewLine);
                await ReplyAsync($"I will now reply if an error is thrown in {Context.Guild}");
            }
        }

        [Command("kicks+")]
        [Summary("kicks+")]
        [Remarks("Users kicked by passivebot")]
        [RequireContext(ContextType.Guild)]
        public async Task Kicks()
        {
            if (!File.Exists(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/kick.txt"))
                await ReplyAsync(
                    "There are currently no kicks in this server, to kick someone type `.kick @user 'reason'`");
            var kicks = File.ReadAllText(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/kick.txt");
            await ReplyAsync("```\n" + kicks + "\n```");
        }

        [Command("warns+")]
        [Summary("warns+")]
        [Remarks("Users warned by passivebot")]
        [RequireContext(ContextType.Guild)]
        public async Task Warns()
        {
            if (!File.Exists(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/warn.txt"))
                await ReplyAsync(
                    "There are currently no warns in this server, to warn someone type `.warn @user 'reason'`");
            var warns = File.ReadAllText(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/warn.txt");
            await ReplyAsync("```\n" + warns + "\n```");
        }

        [Command("bans+")]
        [Summary("bans+")]
        [Remarks("Users banned by passivebot")]
        [RequireContext(ContextType.Guild)]
        public async Task Bans()
        {
            if (!File.Exists(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/warn.txt"))
                await ReplyAsync(
                    "There are currently no bans in this server, to ban someone type `.ban @user 'reason'`");
            var bans = File.ReadAllText(AppContext.BaseDirectory + $"setup/server/{Context.Guild.Id}/warn.txt");
            await ReplyAsync("```\n" + bans + "\n```");
        }
    }
}