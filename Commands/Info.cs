using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Configuration;

namespace PassiveBOT.Commands
{
    public class Information : ModuleBase
    {
        [Command("user")]
        [Summary("user [Optional]<@user>")]
        [Alias("whois", "userinfo")]
        [Remarks("Returns info about the current user, or the given user")]
        public async Task UserInformation([Remainder] [Optional] IUser user)
        {
            if (user == null)
                user = Context.User;
            var status = user.Status.ToString();
            if (status == "")
                status = "Null";

            var builder = new EmbedBuilder()
                .WithTitle($"Who Is: {user}")
                .AddInlineField("Sign Up Date", user.CreatedAt.Date)
                .AddInlineField("User ID", user.Id)
                .AddInlineField("Username", user.Username)
                .AddInlineField("Discriminatior", user.Discriminator)
                .AddInlineField("Status", status)
                .AddField("Links",
                    $"[Site]({Load.Siteurl}) \n[Invite]({Load.Invite})\n[Our Server]({Load.Server})")
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, builder);
        }

        [Command("avatar")]
        [Summary("avatar [Optional]<@user>")]
        [Remarks("Returns the users avatar")]
        public async Task Avatar([Remainder] IUser user = null)
        {
            if (user == null)
                user = Context.User;

            var builder = new EmbedBuilder()
                .WithTitle($"{user}'s Avatar")
                .WithImageUrl(user.GetAvatarUrl())
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, builder);
        }

        [Command("info")]
        [Alias("botinfo")]
        [Summary("info")]
        [Remarks("Display's the bots information")]
        public async Task Info()
        {
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.Name = $"PassiveBOT {Load.Version}";
                    x.Url = "https://goo.gl/s3BZTw";
                })
                .AddInlineField("Author", Load.Owner)
                .AddInlineField("Uptime", GetUptime())
                .AddInlineField("Heap", $"{GetHeapSize()}MB")
                .AddInlineField("Servers", (Context.Client as DiscordSocketClient).Guilds.Count)
                .AddInlineField("Channels", (Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count))
                .AddInlineField("Users", (Context.Client as DiscordSocketClient).Guilds.Sum(g => g.MemberCount))
                .AddField("Links",
                    $"[Site]({Load.Siteurl}) \n[Invite]({Load.Invite})\n[Our Server]({Load.Server})")
                .WithFooter(x =>
                {
                    x.WithText($"PassiveBOT || Messages Recieved Since Last Reboot: {Load.Messages}");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            await ReplyAsync("", false, embed.Build());
        }

        [Command("stats")]
        [Summary("stats")]
        [Remarks("Statistics about passivebot")]
        public async Task Stats()
        {
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.Name = $"PassiveBOT {Load.Version}";
                    x.Url = "https://goo.gl/s3BZTw";
                })
                .WithFooter(x =>
                {
                    x.WithText("PassiveBOT");
                    x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
                });

            embed.AddInlineField("Author", Load.Owner);
            embed.AddInlineField("Uptime", GetUptime());
            embed.AddInlineField("Heap", $"{GetHeapSize()}MB");
            embed.AddInlineField("Guilds", (Context.Client as DiscordSocketClient).Guilds.Count);
            embed.AddInlineField("Channels", (Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count));
            embed.AddInlineField("Users", (Context.Client as DiscordSocketClient).Guilds.Sum(g => g.MemberCount));

            embed.AddField("Links",
                $"[Site]({Load.Siteurl}) \n[Invite]({Load.Invite})\n[Our Server]({Load.Server})");


            await ReplyAsync("", false, embed.Build());
        }

        [Command("Uptime")]
        [Summary("uptime")]
        [Remarks("Current uptime for passivebot since last restart")]
        public async Task Uptime()
        {
            var embed = new EmbedBuilder
            {
                Title = "PassiveBOT Uptime:",
                Description = GetUptime() + "\nDD.HH:MM:SS",
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
            };

            await ReplyAsync("", false, embed.Build());
        }

        [Command("Heap")]
        [Summary("heap")]
        [Remarks("Current Heap Size")]
        public async Task Heap()
        {
            var embed = new EmbedBuilder
            {
                Title = "PassiveBOT Heap Size:",
                Description = GetHeapSize() + "MB",
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
            };

            await ReplyAsync("", false, embed.Build());
        }

        private static string GetUptime()
        {
            return (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        }

        private static string GetHeapSize()
        {
            return Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.InvariantCulture);
        }
    }
}