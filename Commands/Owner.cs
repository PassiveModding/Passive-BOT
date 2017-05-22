using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ImageSharp;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
namespace PassiveBOT.Commands
{
    [RequireOwner]
    public class Owner : ModuleBase
    {
        [Command("die"), Summary("die"), Remarks("Kills the bot (owner only)")]
        public async Task Die()
        {
            await ReplyAsync("Bye Bye :heart:");
            Environment.Exit(1);
        }
        [Command("ServerList"), Summary("serverlist"), Remarks("Gets all the servers the bot is connected to."), Alias("sl")]
        public async Task ServerListAsync()
        {
            var client = Context.Client as DiscordSocketClient;
            var embed = new EmbedBuilder();
            foreach (SocketGuild guild in client.Guilds)
            {
                embed.AddField(x =>
                {
                    x.Name = $"{guild.Name} || {guild.Id} || Guild Members: {guild.MemberCount}";
                    x.Value = $"-------------";
                    x.IsInline = true;
                });
            }
            embed.Title = "=== Server List ===";
            embed.Color = new Discord.Color(255, 210, 50);
            embed.Footer = new EmbedFooterBuilder()
            {
                Text = $"Total Guilds: {client.Guilds.Count.ToString()}"
            };
            await ReplyAsync("", embed: embed);

        }

        [Command("Broadcast"), Summary("broadcast 'message'"), Remarks("Sends a message to ALL severs that the bot is connected to."), Alias("Yell", "Shout")]
        public async Task AsyncBroadcast([Remainder] string msg)
        {
            var glds = (Context.Client as DiscordSocketClient).Guilds;
            var defaultchan = glds.Select(g => g.GetChannel(g.Id)).Cast<ITextChannel>();
            await Task.WhenAll(defaultchan.Select(c => c.SendMessageAsync(msg)));

        }

        [Command("Username"), Summary("username 'name'"), RequireContext(ContextType.Guild), Remarks("Sets the bots username")]
        public async Task UsernameAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be empty");
            var client = Context.Client as DiscordSocketClient;
            await Context.Client.CurrentUser.ModifyAsync(x => x.Username = value).ConfigureAwait(false);
            await ReplyAsync("Bot Username updated").ConfigureAwait(false);

        }
    }
}